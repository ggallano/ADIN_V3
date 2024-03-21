using ADI.Register.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ADI.Register.Services
{
    public class RegisterService : IRegisterService
    {
        public ObservableCollection<RegisterModel> GetAdditionalRegisterSetRev0(ObservableCollection<RegisterModel> registers)
        {
            //ObservableCollection<RegisterModel> registers = new ObservableCollection<RegisterModel>();

            #region Additional Register sets

            registers.Add(
               new RegisterModel()
               {
                   Name = "CRSM_FRM_GEN_DIAG_CLK",
                   Address = 0x1E882C,
                   BitFields = new List<BitFieldModel>()
                   {
                        new BitFieldModel()
                        {
                            Name ="CRSM_FRM_GEN_DIAG_CLK_EN",
                            Start = 1,
                            Width = 1,
                            ResetValue = 0,
                            Value = 0
                        }
                   }
               });

            registers.Add(
                new RegisterModel()
                {
                    Name = "AN_FRC_MODE_EN",
                    Address = 0x078000,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="AN_FRC_MODE_EN",
                            Start = 0,
                            Width = 1,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FG_FRM_LEN",
                    Address = 0x1E801A,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FG_FRM_LEN",
                            Start = 0,
                            Width = 16,
                            ResetValue = 107,
                            Value = 107
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FG_NFRM_L",
                    Address = 0x1E801D,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FG_NFRM_L",
                            Start = 0,
                            Width = 16,
                            ResetValue = 256,
                            Value = 256
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FG_NFRM_H",
                    Address = 0x1E801C,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FG_NFRM_H",
                            Start = 0,
                            Width = 16,
                            ResetValue = 0,
                            Value = 1
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FG_CONT_MODE_EN",
                    Address = 0x1E8017,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FG_CONT_MODE_EN",
                            Start = 0,
                            Width = 1,
                            ResetValue = 0,
                            Value = 1
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FG_CNTRL",
                    Address = 0x1E8016,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FG_CNTRL",
                            Start = 0,
                            Width = 3,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FG_EN",
                    Address = 0x1E8015,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FG_EN",
                            Start = 0,
                            Width = 1,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FC_TX_SEL",
                    Address = 0x1E8005,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FC_TX_SEL",
                            Start = 0,
                            Width = 1,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FG_DONE",
                    Address = 0x1E801E,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FG_DONE",
                            Start = 0,
                            Width = 1,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FC_EN",
                    Address = 0x1E8001,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FC_EN",
                            Start = 0,
                            Width = 1,
                            ResetValue = 1,
                            Value = 1
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "RX_ERR_CNT",
                    Address = 0x1E8008,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="RX_ERR_CNT",
                            Start = 0,
                            Width = 16,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FC_FRM_CNT_L",
                    Address = 0x1E800A,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FC_FRM_CNT_L",
                            Start = 0,
                            Width = 16,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FC_FRM_CNT_H",
                    Address = 0x1E8009,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FC_FRM_CNT_H",
                            Start = 0,
                            Width = 16,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FgDa5Emi",
                    Address = 0x1E8028,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FgDa5Emi",
                            Start = 0,
                            Width = 8,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FgSa",
                    Address = 0x1E8029,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FgSa",
                            Start = 0,
                            Width = 8,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "RMII_CFG",
                    Address = 0x1E8038,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="RMII_TXD_CHK_EN",
                            Start = 0,
                            Width = 1,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            return registers;

            #endregion Additional Register sets
        }

        public ObservableCollection<RegisterModel> GetAdditionalRegisterSetRev1(ObservableCollection<RegisterModel> registers)
        {
            //ObservableCollection<RegisterModel> registers = new ObservableCollection<RegisterModel>();

            #region Additional Register sets

            registers.Add(
               new RegisterModel()
               {
                   Name = "CRSM_FRM_GEN_DIAG_CLK",
                   Address = 0x1E882C,
                   BitFields = new List<BitFieldModel>()
                   {
                        new BitFieldModel()
                        {
                            Name ="CRSM_FRM_GEN_DIAG_CLK_EN",
                            Start = 1,
                            Width = 1,
                            ResetValue = 0,
                            Value = 0
                        }
                   }
               });

            registers.Add(
                new RegisterModel()
                {
                    Name = "AN_FRC_MODE_EN",
                    Address = 0x078000,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="AN_FRC_MODE_EN",
                            Start = 0,
                            Width = 1,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FgSa",
                    Address = 0x1F8033,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FgSa",
                            Start = 0,
                            Width = 8,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            registers.Add(
                new RegisterModel()
                {
                    Name = "FgDa5Emi",
                    Address = 0x1F8032,
                    BitFields = new List<BitFieldModel>()
                    {
                        new BitFieldModel()
                        {
                            Name ="FgDa5Emi",
                            Start = 0,
                            Width = 8,
                            ResetValue = 0,
                            Value = 0
                        }
                    }
                });

            return registers;

            #endregion Additional Register sets
        }

        public Dictionary<string, RegisterModel> GetDictRegister(string registerFileName)
        {
            var registerStruct = JsonConvert.DeserializeObject<RegisterSet>(File.ReadAllText(registerFileName));
            foreach (var register in registerStruct.Registers)
            {
                register.BitFields.RemoveAll(x => x.Name == "RESERVED");
            }

            Dictionary<string, RegisterModel> dictRegisters = new Dictionary<string, RegisterModel>();
            foreach (var register in registerStruct.Registers)
            {
                dictRegisters.Add(register.Name, register);
            }

            return dictRegisters;
        }

        public ObservableCollection<RegisterModel> GetRegisterSet(string registerFileName)
        {
            ObservableCollection<RegisterModel> registerSet = new ObservableCollection<RegisterModel>();
            var registerStruct = JsonConvert.DeserializeObject<RegisterSet>(File.ReadAllText(registerFileName));
            foreach (var register in registerStruct.Registers)
            {
                register.BitFields.RemoveAll(x => x.Name == "RESERVED");
            }
            registerSet = registerStruct.Registers;

            return registerSet;
        }
    }
}