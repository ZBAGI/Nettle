﻿namespace Nettle.Demo.Windows
{
    using Nettle.Compiler;
    using Nettle.Data;
    using Nettle.Data.Database;
    using Nettle.NCalc;
    using Nettle.Web;
    using System;
    using System.Configuration;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class DemoForm : Form
    {
        private INettleCompiler _compiler;

        public DemoForm()
        {
            InitializeComponent();

            var dataResolver = new NettleDataResolver();

            var connectionString = ConfigurationManager.AppSettings
            [
                "DatabaseConnectionString"
            ];

            if (false == String.IsNullOrEmpty(connectionString))
            {
                dataResolver.ConnectionRepository.AddConnection
                (
                    new SqlClientConnection
                    (
                        "Demo",
                        connectionString
                    )
                );
            }

            NettleEngine.RegisterResolvers
            (
                new DefaultNettleResolver(),
                new NettleWebResolver(),
                new NettleNCalcResolver(),
                dataResolver
            );

            _compiler = NettleEngine.GetCompiler();
            
            _compiler.AutoRegisterViews
            (
                "../../Templates"
            );
        }

        private void renderButton_Click
            (
                object sender,
                EventArgs e
            )
        {
            try
            {
                var template = _compiler.Compile
                (
                    templateTextBox.Text
                );

                var model = new
                {
                    Message = "Hello World",
                    Names = new string[]
                    {
                        "Craig",
                        "John",
                        "Simon"
                    },
                    Success = true
                };

                var output = template(model);

                outputTextBox.ForeColor = Color.White;
                outputTextBox.Text = output;
            }
            catch (Exception ex)
            {
                outputTextBox.ForeColor = Color.Red;
                outputTextBox.Text = ex.Message;
            }
        }
    }
}
