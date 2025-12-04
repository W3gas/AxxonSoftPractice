using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AxxonSoft_Prac
{
    public class ControlLoadException : Exception
    {
        public IReadOnlyList<string> MissingControls { get; }


        public ControlLoadException(IEnumerable<string> controlNames, Exception? innerException = null)
            : base($"UI load error: controls not found -> {string.Join(", ", controlNames)}", innerException)
        {
            MissingControls = new List<string>(controlNames);
        }

        
    }
}
