# MyMvcApp

## Overview
MyMvcApp is an ASP.NET MVC application designed to demonstrate the structure and functionality of a typical MVC project. It includes a simple home page and a sample data model.

## Project Structure
```
MyMvcApp
├── Controllers
│   └── HomeController.cs
├── Models
│   └── SampleModel.cs
├── Views
│   ├── Home
│   │   └── Index.cshtml
│   └── Shared
│       └── _Layout.cshtml
├── wwwroot
│   ├── css
│   │   └── site.css
│   └── js
│       └── site.js
├── appsettings.json
├── Program.cs
├── Startup.cs
└── README.md
```

## Setup Instructions
1. Clone the repository to your local machine.
2. Open the project in your preferred IDE.
3. Restore the NuGet packages by running `dotnet restore`.
4. Run the application using `dotnet run`.
5. Navigate to `http://localhost:5000` in your web browser to view the home page.

## Features
- Home page with a simple greeting.
- Sample data model to demonstrate MVC functionality.
- Responsive design with CSS styling.
- JavaScript for enhanced user interaction.

## Technologies Used
- ASP.NET Core MVC
- C#
- Razor Views
- HTML/CSS/JavaScript

## License
This project is licensed under the MIT License.