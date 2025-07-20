# OperatorEntryApp

OperatorEntryApp is a Windows Forms desktop application developed during an internship at Haier Europe.

## Overview

This application provides a barcode entry and user management system for factory personnel. Key features include:

- Login/Register with SHA-256 hashed passwords  
- Role-based authorization (engineer, operator, supervisor)  
- Product barcode and supplier code entry with timestamp  
- Users can change their own passwords  
- Admin users can reset passwords for other users  
- Displays last 5 user entries  
- Validations for password length (minimum 6 characters) and barcode length (maximum 8 characters)  
- SQL Server database integration using ADO.NET  
- Full Turkish UI, compatible with industrial touchscreen tablets  

## Technologies Used

- C# (.NET Framework 4.8)  
- Windows Forms (WinForms)  
- Microsoft SQL Server (LocalDB or SQLExpress)  
- ADO.NET  
- Git  

## Database Schema

**Users**  
- `UserId`  
- `FullName`  
- `Password`  
- `Role`  

**UserInputs**  
- `InputId`  
- `ProductBarcode`  
- `SupplierCode`  
- `UserId` (Foreign Key)  
- `Timestamp`  

**Logs** *(optional)*  
- `LogId`  
- `UserId` (Foreign Key)  
- `Action`  
- `Timestamp`  

## Setup Instructions

1. Create or restore a SQL Server database named `Haier_DB`.  
2. Update the connection string in the `App.config` file:
   ```xml
   <connectionStrings>
     <add name="Haier_DB" connectionString="your_connection_string_here" />
   </connectionStrings>
3. Open the solution file OperatorEntryApp.sln in Visual Studio.

4. Build and run the application on a system with .NET Framework 4.8 installed.
    
5. Ensure the Users and UserInputs tables exist in the database.

Additional Notes
The Enter key has been disabled on the password change form to prevent unintended submissions.

All fields are required, and validations will warn the user if any field is left empty.

All user-facing messages are presented in Turkish for compatibility with the target users.

Author
Developed by Emre Güner in 2025 during an internship at Haier Europe – Digitalization Department.