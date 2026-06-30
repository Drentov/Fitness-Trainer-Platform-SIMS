using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FitnessTrainerPlatform.Models;

namespace FitnessTrainerPlatform.Services;

public static class AuthService
{
    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool VerifyPassword(string password, string hash) =>
        HashPassword(password).Equals(hash, StringComparison.OrdinalIgnoreCase);
}

public interface IDataStore //An interface, so we can switch this out to SQL later
{
    string DataDirectory { get; }
    string DataFilePath { get; }
    AppData Data { get; }
    void Save();
    void Reload();
    UserAccount? FindUser(string username);
    UserAccount? FindUserById(string id);
    TrainerProfile? FindTrainer(string trainerId);
    TrainerProfile? FindTrainerByUserId(string userId);
    List<TrainerProfile> GetApprovedTrainers();
}

public class DataStore : IDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _dataDirectory;
    private readonly string _dataFilePath;
    private AppData _data = new();
    private readonly object _lock = new();

    public DataStore()
    {
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FitnessTrainerPlatform");
        Directory.CreateDirectory(_dataDirectory);
        _dataFilePath = Path.Combine(_dataDirectory, "appdata.json");
        Load();
    }

    public string DataDirectory => _dataDirectory;
    public string DataFilePath => _dataFilePath;

    public AppData Data
    {
        get { lock (_lock) return _data; }
    }

    public void Save()
    {
        lock (_lock)
        {
            var json = JsonSerializer.Serialize(_data, JsonOptions);
            File.WriteAllText(_dataFilePath, json, Encoding.UTF8);
        }
    }

    public void Reload()
    {
        lock (_lock) Load();
    }

    private void Load()
    {
        if (!File.Exists(_dataFilePath))
        {
            _data = SeedDataService.CreateInitialData();
            Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(_dataFilePath, Encoding.UTF8);
            _data = JsonSerializer.Deserialize<AppData>(json, JsonOptions) ?? new AppData();
        }
        catch
        {
            _data = SeedDataService.CreateInitialData();
            Save();
        }
    }

    public UserAccount? FindUser(string username) =>
        _data.Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    public UserAccount? FindUserById(string id) =>
        _data.Users.FirstOrDefault(u => u.Id == id);

    public TrainerProfile? FindTrainer(string trainerId) =>
        _data.Trainers.FirstOrDefault(t => t.Id == trainerId);

    public TrainerProfile? FindTrainerByUserId(string userId) =>
        _data.Trainers.FirstOrDefault(t => t.UserAccountId == userId);

    public List<TrainerProfile> GetApprovedTrainers() =>
        _data.Trainers.Where(t => t.ApprovalStatus == TrainerApprovalStatus.Approved && t.HasPaidMonthlyFee).ToList();
}
