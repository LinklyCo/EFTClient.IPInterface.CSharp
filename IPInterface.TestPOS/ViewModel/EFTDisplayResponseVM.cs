﻿using System.ComponentModel;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel
{
    public class EFTDisplayResponseVM
    {
        /// <summary>Constructs a default display response object.</summary>
        public EFTDisplayResponseVM(EFTDisplayResponse m)
        {
            NumberOfLines = m.NumberOfLines;
            LineLength = m.LineLength;
            DisplayText = m.DisplayText;
            CancelKeyFlag = m.CancelKeyFlag;
            AcceptYesKeyFlag = m.AcceptYesKeyFlag;
            DeclineNoKeyFlag = m.DeclineNoKeyFlag;
            AuthoriseKeyFlag = m.AuthoriseKeyFlag;
            OKKeyFlag = m.OKKeyFlag;
            InputType = m.InputType;
            GraphicCode = m.GraphicCode;
            PurchaseAnalysisData = m.PurchaseAnalysisData;
        }

        /// <summary>Number of lines to display.</summary>
        /// <value>Type: <see cref="System.Int32" /></value>
        public int NumberOfLines { get; set; }

        /// <summary>Number of character per display line.</summary>
        /// <value>Type: <see cref="System.Int32" /></value>
        public int LineLength { get; set; }

        /// <summary>Text to be displayed. Each display line is concatenated.</summary>
        /// <value>Type: <see cref="System.String" >String array</see></value>
        public string[] DisplayText { get; set; }

        /// <summary>Indicates whether the Cancel button is to be displayed.</summary>
        /// <value>Type: <see cref="System.Boolean" /></value>
        public bool CancelKeyFlag { get; set; } = false;

        /// <summary>Indicates whether the Accept/Yes button is to be displayed.</summary>
        /// <value>Type: <see cref="System.Boolean" /></value>
        public bool AcceptYesKeyFlag { get; set; } = false;

        /// <summary>Indicates whether the Decline/No button is to be displayed.</summary>
        /// <value>Type: <see cref="System.Boolean" /></value>
        public bool DeclineNoKeyFlag { get; set; } = false;

        /// <summary>Indicates whether the Authorise button is to be displayed.</summary>
        /// <value>Type: <see cref="System.Boolean" /></value>
        public bool AuthoriseKeyFlag { get; set; } = false;

        /// <summary>Indicates whether the OK button is to be displayed.</summary>
        /// <value>Type: <see cref="System.Boolean" /></value>
        public bool OKKeyFlag { get; set; } = false;

        public InputType InputType { get; set; }

        public GraphicCode GraphicCode { get; set; }

        public PadField PurchaseAnalysisData { get; set; }
    }
}
