using Microsoft.AspNetCore.Mvc;
using RiscVEmulator.Models;
using RiscVEmulator.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace RiscVEmulator.Controllers
{
    public class EmulatorController : Controller
    {
        private readonly EmulatorContext _context;

        public EmulatorController(EmulatorContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var emulator = new Emulator();
            var viewModel = new EmulatorViewModel
            {
                Program = new ProgramRecord(),
                Registers = emulator.Regs,
                Memory = emulator.Memory.Skip(0x1000).Take(16).ToArray(),
                Pc = emulator.Pc
            };
            ViewBag.InstructionSet = Emulator.InstructionSet;
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Run(ProgramRecord program)
        {
            var emulator = new Emulator();
            try
            {
                emulator.LoadProgram(program.Code);
                while (emulator.Pc < 0x1000 + program.Code.Split('\n').Length * 4)
                    emulator.ExecuteInstruction();

                program.CreatedAt = DateTime.Now;
                _context.Programs.Add(program);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                var viewModel = new EmulatorViewModel
                {
                    Program = program,
                    Registers = emulator.Regs,
                    Memory = emulator.Memory.Skip(0x1000).Take(16).ToArray(),
                    Pc = emulator.Pc,
                    ErrorMessage = ex.Message
                };
                ViewBag.InstructionSet = Emulator.InstructionSet;
                return View("Index", viewModel);
            }

            var successViewModel = new EmulatorViewModel
            {
                Program = program,
                Registers = emulator.Regs,
                Memory = emulator.Memory.Skip(0x1000).Take(16).ToArray(),
                Pc = emulator.Pc
            };
            ViewBag.InstructionSet = Emulator.InstructionSet;
            return View("Index", successViewModel);
        }

        [HttpPost]
        public IActionResult Step(ProgramRecord program)
        {
            var emulator = new Emulator();
            try
            {
                emulator.LoadProgram(program.Code);
                emulator.ExecuteInstruction();
            }
            catch (Exception ex)
            {
                var viewModel = new EmulatorViewModel
                {
                    Program = program,
                    Registers = emulator.Regs,
                    Memory = emulator.Memory.Skip(0x1000).Take(16).ToArray(),
                    Pc = emulator.Pc,
                    ErrorMessage = ex.Message
                };
                ViewBag.InstructionSet = Emulator.InstructionSet;
                return View("Index", viewModel);
            }

            var successViewModel = new EmulatorViewModel
            {
                Program = program,
                Registers = emulator.Regs,
                Memory = emulator.Memory.Skip(0x1000).Take(16).ToArray(),
                Pc = emulator.Pc
            };
            ViewBag.InstructionSet = Emulator.InstructionSet;
            return View("Index", successViewModel);
        }

        public IActionResult History()
        {
            var programs = _context.Programs.OrderByDescending(p => p.CreatedAt).ToList();
            return View(programs);
        }
    }
}