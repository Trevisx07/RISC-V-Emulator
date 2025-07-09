# RISC-V Emulator Web Application

![.NET](https://img.shields.io/badge/.NET-ASP.NET%20Core-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Platform](https://img.shields.io/badge/platform-Web%20App-lightgrey)

## 🚀 Overview

This is a web-based **RISC-V Emulator** developed using **ASP.NET Core MVC**. It allows users to write, load, and execute RISC-V assembly programs directly from a web interface, with real-time updates to registers, memory, and output.

> Built by **Het Patel** for educational and experimental purposes — ideal for students, hobbyists, and developers exploring RISC-V architecture.

---

## ⚙️ Features

- 🧠 **Instruction Parsing & Execution** for core RISC-V operations  
- 📝 **Code Editor Interface** for writing and submitting RISC-V programs  
- 🗃️ **Registers and Memory View** with real-time updates  
- 💥 **Error Handling** with clear feedback  
- 💾 **SQLite database integration** for storing user programs  
- 🌐 Built using ASP.NET Core MVC, Entity Framework, and Razor Pages

---

## 🖥️ Screenshots

> _(Add your own screenshots here — UI, memory/registers view, etc.)_

---

## 📦 Technologies Used

- **ASP.NET Core MVC**
- **Entity Framework Core (SQLite)**
- **Razor Pages**
- **C#**
- **Bootstrap / HTML / CSS / JS**

---

## 📁 Project Structure

RiscVEmulator/
├── Controllers/
├── Models/
├── Views/
│ └── Emulator/
├── wwwroot/
├── Data/
└── Program.cs

yaml
Copy
Edit

---

## 🚀 Getting Started

### Prerequisites

- [.NET SDK 7.0+](https://dotnet.microsoft.com/download)
- Visual Studio / VS Code

### Run Locally

```bash
git clone https://github.com/Trevisx07/RISC-V-Emulator.git
cd RISC-V-Emulator
dotnet run
Then open your browser at:
http://localhost:5290

📚 Supported Instructions
Supports a range of RISC-V base instructions, including but not limited to:

addi, sub, and, or, xor, sll, srl, sra

lw, sw

beq, bne, j, jal, jr

More instructions will be added in future versions.

❗ Known Limitations
Partial RISC-V instruction set

Limited exception handling

No file I/O or advanced syscall support yet

📜 License
This project is licensed under the MIT License.
See the LICENSE file for details.

👤 Author & Contact
Het Patel
📧 Email: hetkumarpatel07@gmail.com

If you have questions, issues, or want to collaborate — feel free to reach out!

⭐ Contributing
Contributions, ideas, and suggestions are welcome!
Please open an issue or pull request.

🙌 Acknowledgments
RISC-V ISA documentation

Microsoft ASP.NET Core community

All open-source developers contributing to web-based emulation

Thank you for checking out this project!

### ✅ How to Use:
1. Save that as `README.md` in your project root folder.
2. Commit and push it to GitHub:

```bash
git add README.md
git commit -m "Add detailed README"
git push
