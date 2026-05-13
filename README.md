# EasySave - User Manual & Technical Documentation

***

## What is EasySave?

EasySave is a backup software project developed for **ProSoft**, a software publishing company. The project is managed under the responsibility of the CIO and follows the technical, documentation, and maintainability standards defined for all ProSoft Suite applications.

***

## Getting Started

### How to Launch

Navigate to the root of the EasySave project in your Command Prompt, then run one of the following commands:

```bash
dotnet run -- "[args]"
```

Or use the compiled executable directly:

```
C:\Users\User\Desktop\EasySave\bin\Debug\net8.0-windows\EasySave.exe "[args]"
```

> ⚠️ The `Desktop` path may vary depending on where you installed EasySave. Use the commands below if you're unsure of your location:
>
> - `echo %cd%` - Print your current directory
> - `dir /s EasySave` - Search for the EasySave folder

### Launch Arguments

| Argument | Description |
|----------|-------------|
| *(none)* | Launches the full interactive console application |
| `1-3` | Runs jobs 1 through 3 |
| `1;3` | Runs job 1 **AND** job 3 |
| `1-3;5` | Runs jobs 1 through 3 **AND** job 5 |

***

## Backup Job Management

In this first part you have the **Backup Job Management** with the list of your jobs. You can create, delete, execute (one or more jobs) and modify a job.

### Create a Backup Job

Press the button `Add` to create a new backup job. You will be prompted to define:
- A job **name**
- The **Source** and **Target** directories
- A [**Backup Type**](#backup-types) (Full or Differential)

### Run a Backup Job

Press the button `Run` after having checked all the jobs you want to run. For example, if you want to run only the first one, you can checkbox the first one and press the button. If you want to run jobs 1 and 2, you can check the boxes and press the button. 

### Delete a Backup Job

Press `Delete` to delete an existing backup job.

***

## Backup Types

EasySave supports two types of backup:

| Type | Description |
|------|-------------|
| **Full** | Complete duplication of the source to the destination. All files are copied regardless of previous backups. |
| **Differential** | Only new or modified files are copied. If a file has been updated since the last backup, it will be overwritten on the destination. |

***

## Execution
In the **Execution** part, you will be able to see the details of your job's execution. 

If you launch multiple jobs at the same time you will be able to see either the global job execution and also the sub-jobs you have selected.

***

## Settings 

In the **Settings** part, you will be able to change multiple things:
- **Language**, between French and English
- **Log Format**, between .json and .xml
- **Buisness Application**
- **Ecnryption key**
- **Encrypted extensions**

At the end, make sure to press the `Save` button.

***


## Technical Support

### Software Overview

EasySave provides a reliable, maintainable, and user-friendly backup solution for end users.

### Minimum Configuration

| Requirement | Details |
|-------------|---------|
| **OS** | Windows 10 / 11 |
| **Runtime** | .NET 8.0 |
| **Disk Space** | ~50 MB |

### Installation & Default Location

Navigate to the root of the EasySave project and execute the [launch command](#how-to-launch) as described above. No additional installer is required.

***

## Architecture & Design Patterns

### Class Diagram

> 📄 See the attached PDF file for the full class diagram.

### Design Patterns Used

#### Strategy - Backup Behavior
Used by `BackupJob` to decide how to select and copy files. Allows swapping between Full and Differential backup logic without modifying the core job class.

#### Factory - Job Creation / Strategy Selection
- Creates `BackupJob` instances from configuration or user input
- Chooses the appropriate `IBackupStrategy` (full or differential)
- Integrated into the `BackupJobManager`

#### Singleton - Shared Services
Ensures a single, consistent configuration source across the application lifetime.

#### Observer - Real-Time Status Updates
`BackupJob` notifies registered observers on progress and events, enabling live feedback in the console.

#### Bridge - Storage Abstraction
`BackupJob` works with an `IStorage` interface instead of concrete file system types, allowing support for local, external, and network storage.

### Sequence Diagram

> 📄 See the attached PDF file for the full sequence diagram.

***

## Known Limitations

The following features are **not yet implemented** in the current version:

