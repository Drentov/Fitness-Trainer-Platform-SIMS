# FitTrain Platform

A desktop fitness trainer marketplace (Steam + dating app for trainers), built for the SIMS university project.

## Tech Stack

- **C# / WPF** (.NET 8)
- **Plain JSON file storage** through a small data-store abstraction (no real database yet)
- **TCP inter-process chat** on localhost port 9876

## Run

```bash
cd FitnessTrainerPlatform
dotnet run --project FitnessTrainerPlatform
```

Or open `FitnessTrainerPlatform.slnx` in Visual Studio and press F5.

## Demo Accounts

| Role | Username | Password |
|------|----------|----------|
| Admin | admin | admin123 |
| User | john | user123 |
| Trainer | coach_mike | trainer123 |
| Pending trainer | coach_new | trainer123 |

## Demo Flow

1. **Login as `john`** - browse trainers, view profile, request tutelage (Wish Sheet popup).
2. **Login as `coach_mike`** (second app instance) - accept request, assign training from exercise library, open chat.
3. **Back as `john`** - complete training reply, open chat to see IPC messages.
4. **Login as `admin`** - approve pending trainer `coach_new`, demo storage tab.

### Inter-Process Chat Demo

Launch the app **twice**. Log in as trainer in one window and user in the other. After tutelage is accepted, open chat from both sides; messages sent in one window appear in the other via TCP IPC.

## Features Implemented

- User registration & login (SHA-256 password hashing)
- Browse approved trainers (rating, reviews, qualifications, prerequisites, fee)
- Tutelage requests with User Wish Sheet
- Trainer accept/deny tutelage
- Reusable exercise library with video path
- Training assignment & user replies (stars + opinion)
- Messenger-style internal chat with IPC
- Admin trainer approval, fee warnings, removal
- Trainer monthly platform fee (dummy)
- Plain local JSON data storage, easy to replace later with a SQL-backed `IDataStore`

## Data Location

Plain JSON data: `%LocalAppData%\FitnessTrainerPlatform\appdata.json`

Delete this file to reset seed data.

## Team Roles (from project plan)

- UI - Aron
- Chat / IPC - Aleksa
- Core classes - Sofija & Milica
- Data layer - Filip
