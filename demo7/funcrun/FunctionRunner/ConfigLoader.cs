using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionRunner
{
    public class Config
    {
        public Keyvault? Keyvault { get; set; }
    }

    public class Keyvault
    {
        public required string Name { get; set; }
        public VaultObject[]? Secrets { get; set; }
        public VaultObject[]? Certificates { get; set; }
        public VaultObject[]? Keys { get; set; }
    }

    public class VaultObject
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required string Value { get; set; }
    }

    internal class ConfigLoader
    {
    }
}
