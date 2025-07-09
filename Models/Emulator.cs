using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RiscVEmulator.Models
{
    public class Emulator
    {
        private const int MEM_SIZE = 0x10000; // 64KB
        public uint[] Regs { get; set; } = new uint[32];
        public uint Pc { get; set; } = 0x1000;
        public byte[] Memory { get; set; } = new byte[MEM_SIZE];

        public static readonly Dictionary<string, string> InstructionSet = new Dictionary<string, string>
        {
            {"add", "add x3 x1 x2  # x3 = x1 + x2"},
            {"sub", "sub x3 x1 x2  # x3 = x1 - x2"},
            {"sll", "sll x3 x1 x2  # x3 = x1 << x2"},
            {"slt", "slt x3 x1 x2  # x3 = (x1 < x2) ? 1 : 0"},
            {"sltu", "sltu x3 x1 x2  # x3 = (x1 < x2, unsigned) ? 1 : 0"},
            {"xor", "xor x3 x1 x2  # x3 = x1 ^ x2"},
            {"srl", "srl x3 x1 x2  # x3 = x1 >> x2"},
            {"sra", "sra x3 x1 x2  # x3 = x1 >> x2 (arithmetic)"},
            {"or", "or x3 x1 x2    # x3 = x1 | x2"},
            {"and", "and x3 x1 x2  # x3 = x1 & x2"},
            {"addi", "addi x1 x0 5  # x1 = x0 + 5"},
            {"slti", "slti x2 x1 10  # x2 = (x1 < 10) ? 1 : 0"},
            {"sltiu", "sltiu x2 x1 10  # x2 = (x1 < 10, unsigned) ? 1 : 0"},
            {"xori", "xori x3 x1 7  # x3 = x1 ^ 7"},
            {"ori", "ori x4 x3 3    # x4 = x3 | 3"},
            {"andi", "andi x5 x4 15  # x5 = x4 & 15"},
            {"slli", "slli x6 x1 2  # x6 = x1 << 2"},
            {"srli", "srli x7 x6 1  # x7 = x6 >> 1"},
            {"srai", "srai x8 x6 1  # x8 = x6 >> 1 (arithmetic)"},
            {"lb", "lb x9 0(x0)    # x9 = signed byte from memory[x0 + 0]"},
            {"lh", "lh x10 0(x0)   # x10 = signed halfword from memory[x0 + 0]"},
            {"lw", "lw x11 0(x0)   # x11 = word from memory[x0 + 0]"},
            {"lbu", "lbu x12 0(x0)  # x12 = unsigned byte from memory[x0 + 0]"},
            {"lhu", "lhu x13 0(x0)  # x13 = unsigned halfword from memory[x0 + 0]"},
            {"jal", "jal x14 8     # x14 = PC + 4, PC += 8"}
        };

        public Emulator()
        {
            Array.Clear(Regs, 0, Regs.Length);
            Array.Clear(Memory, 0, Memory.Length);
        }

        public void LoadProgram(string code)
        {
            var lines = code.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            int addr = 0x1000;

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(p => p.Trim())
                               .ToArray();
                if (parts.Length == 0) continue;
                uint instr = AssembleInstruction(parts);
                BitConverter.GetBytes(instr).CopyTo(Memory, addr);
                addr += 4;
            }
        }

        public void ExecuteInstruction()
        {
            if (Pc >= MEM_SIZE || Pc < 0x1000)
                throw new Exception($"PC out of bounds: 0x{Pc:X4}");

            uint instr;
            try
            {
                instr = BitConverter.ToUInt32(Memory, (int)Pc);
            }
            catch (ArgumentException)
            {
                throw new Exception($"Invalid memory access at PC: 0x{Pc:X4}");
            }

            uint opcode = instr & 0x7F;
            uint rd = (instr >> 7) & 0x1F;
            uint funct3 = (instr >> 12) & 0x7;
            uint rs1 = (instr >> 15) & 0x1F;
            uint rs2 = (instr >> 20) & 0x1F;
            uint funct7 = instr >> 25;
            int imm = (int)(instr >> 20);
            int shamt = (int)(instr >> 20) & 0x1F;

            switch (opcode)
            {
                case 0x13: // I-type
                    Pc += 4;
                    switch (funct3)
                    {
                        case 0x0: Regs[rd] = (uint)(Regs[rs1] + (imm << 20 >> 20)); break; // ADDI
                        case 0x2: Regs[rd] = ((int)Regs[rs1] < (imm << 20 >> 20)) ? 1U : 0U; break; // SLTI
                        case 0x3: Regs[rd] = (Regs[rs1] < (uint)(imm << 20 >> 20)) ? 1U : 0U; break; // SLTIU
                        case 0x4: Regs[rd] = Regs[rs1] ^ (uint)(imm << 20 >> 20); break; // XORI
                        case 0x6: Regs[rd] = Regs[rs1] | (uint)(imm << 20 >> 20); break; // ORI
                        case 0x7: Regs[rd] = Regs[rs1] & (uint)(imm << 20 >> 20); break; // ANDI
                        case 0x1: if (shamt > 31) throw new Exception($"Invalid shift amount {shamt} in SLLI"); Regs[rd] = Regs[rs1] << shamt; break; // SLLI
                        case 0x5:
                            if (shamt > 31) throw new Exception($"Invalid shift amount {shamt} in SRLI/SRAI");
                            if (funct7 == 0x00) Regs[rd] = Regs[rs1] >> shamt; // SRLI
                            else if (funct7 == 0x20) Regs[rd] = (uint)((int)Regs[rs1] >> shamt); // SRAI
                            else throw new Exception($"Invalid funct7 0x{funct7:X2} for opcode 0x13, funct3 0x5");
                            break;
                        default: throw new Exception($"Unknown funct3 0x{funct3:X1} for I-type instruction at PC: 0x{Pc:X4}");
                    }
                    break;

                case 0x03: // Load Instructions
                    Pc += 4;
                    int addr = (int)(Regs[rs1] + (imm << 20 >> 20));
                    if (addr < 0 || addr >= MEM_SIZE || (funct3 == 0x2 && addr % 4 != 0) || (funct3 == 0x1 && addr % 2 != 0))
                        throw new Exception($"Invalid memory address 0x{addr:X4} for load operation");
                    switch (funct3)
                    {
                        case 0x0: Regs[rd] = (uint)(sbyte)Memory[addr]; break; // LB
                        case 0x1: Regs[rd] = (uint)(short)BitConverter.ToInt16(Memory, addr); break; // LH
                        case 0x2: Regs[rd] = BitConverter.ToUInt32(Memory, addr); break; // LW
                        case 0x4: Regs[rd] = Memory[addr]; break; // LBU
                        case 0x5: Regs[rd] = BitConverter.ToUInt16(Memory, addr); break; // LHU
                        default: throw new Exception($"Unknown funct3 0x{funct3:X1} for load instruction at PC: 0x{Pc:X4}");
                    }
                    break;

                case 0x33: // R-type
                    Pc += 4;
                    switch (funct3)
                    {
                        case 0x0:
                            if (funct7 == 0x00) Regs[rd] = Regs[rs1] + Regs[rs2]; // ADD
                            else if (funct7 == 0x20) Regs[rd] = Regs[rs1] - Regs[rs2]; // SUB
                            else throw new Exception($"Invalid funct7 0x{funct7:X2} for ADD/SUB");
                            break;
                        case 0x1: Regs[rd] = Regs[rs1] << (int)(Regs[rs2] & 0x1F); break; // SLL
                        case 0x2: Regs[rd] = ((int)Regs[rs1] < (int)Regs[rs2]) ? 1U : 0U; break; // SLT
                        case 0x3: Regs[rd] = (Regs[rs1] < Regs[rs2]) ? 1U : 0U; break; // SLTU
                        case 0x4: Regs[rd] = Regs[rs1] ^ Regs[rs2]; break; // XOR
                        case 0x5:
                            if (funct7 == 0x00) Regs[rd] = Regs[rs1] >> (int)(Regs[rs2] & 0x1F); // SRL
                            else if (funct7 == 0x20) Regs[rd] = (uint)((int)Regs[rs1] >> (int)(Regs[rs2] & 0x1F)); // SRA
                            else throw new Exception($"Invalid funct7 0x{funct7:X2} for SRL/SRA");
                            break;
                        case 0x6: Regs[rd] = Regs[rs1] | Regs[rs2]; break; // OR
                        case 0x7: Regs[rd] = Regs[rs1] & Regs[rs2]; break; // AND
                        default: throw new Exception($"Unknown funct3 0x{funct3:X1} for R-type instruction at PC: 0x{Pc:X4}");
                    }
                    break;

                case 0x6F: // JAL
                    imm = (int)(((uint)(instr >> 31) << 19) | ((uint)((instr >> 12) & 0xFF) << 11) | ((uint)((instr >> 21) & 0x3FF) << 1) | (uint)((instr >> 20) & 0x1));
                    imm = (imm << 11) >> 11;
                    if (imm % 4 != 0) throw new Exception($"Unaligned jump offset {imm} in JAL");
                    Regs[rd] = Pc;
                    Pc += (uint)imm;
                    break;

                default:
                    throw new Exception($"Unknown opcode: 0x{opcode:X2} at PC: 0x{Pc:X4}");
            }
            Regs[0] = 0;
        }

        private uint AssembleInstruction(string[] parts)
        {
            if (parts.Length < 2) throw new Exception($"Invalid instruction format: {string.Join(" ", parts)}");
            string op = parts[0].ToLower();

            if (!InstructionSet.ContainsKey(op))
            {
                string supportedList = "Supported instructions:\n" + string.Join("\n", InstructionSet.Values);
                throw new Exception($"Invalid instruction '{op}'. Only RV32I instructions are supported.\n{supportedList}");
            }

            try
            {
                return op switch
                {
                    "addi" => AssembleIType(parts, 0x13, 0x0),
                    "slti" => AssembleIType(parts, 0x13, 0x2),
                    "sltiu" => AssembleIType(parts, 0x13, 0x3),
                    "xori" => AssembleIType(parts, 0x13, 0x4),
                    "ori" => AssembleIType(parts, 0x13, 0x6),
                    "andi" => AssembleIType(parts, 0x13, 0x7),
                    "slli" => AssembleShift(parts, 0x13, 0x1, 0x00),
                    "srli" => AssembleShift(parts, 0x13, 0x5, 0x00),
                    "srai" => AssembleShift(parts, 0x13, 0x5, 0x20),
                    "lb" => AssembleLoad(parts, 0x03, 0x0),
                    "lh" => AssembleLoad(parts, 0x03, 0x1),
                    "lw" => AssembleLoad(parts, 0x03, 0x2),
                    "lbu" => AssembleLoad(parts, 0x03, 0x4),
                    "lhu" => AssembleLoad(parts, 0x03, 0x5),
                    "add" => AssembleRType(parts, 0x33, 0x0, 0x00),
                    "sub" => AssembleRType(parts, 0x33, 0x0, 0x20),
                    "sll" => AssembleRType(parts, 0x33, 0x1, 0x00),
                    "slt" => AssembleRType(parts, 0x33, 0x2, 0x00),
                    "sltu" => AssembleRType(parts, 0x33, 0x3, 0x00),
                    "xor" => AssembleRType(parts, 0x33, 0x4, 0x00),
                    "srl" => AssembleRType(parts, 0x33, 0x5, 0x00),
                    "sra" => AssembleRType(parts, 0x33, 0x5, 0x20),
                    "or" => AssembleRType(parts, 0x33, 0x6, 0x00),
                    "and" => AssembleRType(parts, 0x33, 0x7, 0x00),
                    "jal" => AssembleJal(parts),
                    _ => throw new Exception($"Unexpected error assembling '{op}'.")
                };
            }
            catch (FormatException ex)
            {
                throw new Exception($"Invalid number format in instruction: {string.Join(" ", parts)}\nDetails: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Invalid argument in instruction: {string.Join(" ", parts)}\nDetails: {ex.Message}");
            }
        }

        private uint ParseReg(string reg)
        {
            string cleanedReg = reg.Trim('x').TrimEnd(',');
            if (!uint.TryParse(cleanedReg, out uint regNum) || regNum > 31)
                throw new Exception($"Invalid register '{reg}' (must be x0-x31)");
            return regNum;
        }

        private uint AssembleIType(string[] parts, uint opcode, uint funct3)
        {
            if (parts.Length != 4) throw new Exception($"Invalid I-type format: {string.Join(" ", parts)} (expected: op rd rs1 imm)");
            uint rd = ParseReg(parts[1]);
            uint rs1 = ParseReg(parts[2]);
            if (!int.TryParse(parts[3], out int imm) || imm < -2048 || imm > 2047)
                throw new Exception($"Immediate value '{parts[3]}' out of range (-2048 to 2047)");
            return (uint)((imm & 0xFFF) << 20 | rs1 << 15 | funct3 << 12 | rd << 7 | opcode);
        }

        private uint AssembleShift(string[] parts, uint opcode, uint funct3, uint funct7)
        {
            if (parts.Length != 4) throw new Exception($"Invalid shift format: {string.Join(" ", parts)} (expected: op rd rs1 shamt)");
            uint rd = ParseReg(parts[1]);
            uint rs1 = ParseReg(parts[2]);
            if (!uint.TryParse(parts[3], out uint shamt) || shamt > 31)
                throw new Exception($"Shift amount '{parts[3]}' out of range (0-31)");
            return (uint)(funct7 << 25 | shamt << 20 | rs1 << 15 | funct3 << 12 | rd << 7 | opcode);
        }

        private uint AssembleRType(string[] parts, uint opcode, uint funct3, uint funct7)
        {
            if (parts.Length != 4) throw new Exception($"Invalid R-type format: {string.Join(" ", parts)} (expected: op rd rs1 rs2)");
            uint rd = ParseReg(parts[1]);
            uint rs1 = ParseReg(parts[2]);
            uint rs2 = ParseReg(parts[3]);
            return (uint)(funct7 << 25 | rs2 << 20 | rs1 << 15 | funct3 << 12 | rd << 7 | opcode);
        }

        private uint AssembleLoad(string[] parts, uint opcode, uint funct3)
        {
            if (parts.Length != 3) throw new Exception($"Invalid load format: {string.Join(" ", parts)} (expected: op rd offset(rs1))");
            uint rd = ParseReg(parts[1]);
            var match = Regex.Match(parts[2], @"(-?\d+)\(x(\d+)\)");
            if (!match.Success) throw new Exception($"Invalid load offset format: {parts[2]} (expected: offset(xN))");
            if (!int.TryParse(match.Groups[1].Value, out int imm) || imm < -2048 || imm > 2047)
                throw new Exception($"Load offset '{match.Groups[1].Value}' out of range (-2048 to 2047)");
            uint rs1 = ParseReg($"x{match.Groups[2].Value}");
            return (uint)((imm & 0xFFF) << 20 | rs1 << 15 | funct3 << 12 | rd << 7 | opcode);
        }

        private uint AssembleJal(string[] parts)
        {
            if (parts.Length != 3) throw new Exception($"Invalid JAL format: {string.Join(" ", parts)} (expected: jal rd imm)");
            uint rd = ParseReg(parts[1]);
            if (!int.TryParse(parts[2], out int imm) || imm < -1048576 || imm > 1048575 || imm % 2 != 0)
                throw new Exception($"JAL offset '{parts[2]}' out of range (-1048576 to 1048575) or not aligned to 2");
            return (uint)(((uint)(imm >> 20 & 1) << 31) | ((uint)(imm >> 1 & 0x3FF) << 21) | ((uint)(imm >> 11 & 1) << 20) | ((uint)(imm >> 12 & 0xFF) << 12) | (rd << 7) | 0x6F);
        }
    }
}