using Xunit;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace PSTests
{
    // NOTE: do not call AddCommand("out-host") after invoking or MergeMyResults,
    // otherwise Invoke will not return any objects

    public class RunspaceTests
    {
        private static int count = 1;
        private static string script = String.Format($"get-command get-command");

        [Fact]
        public void TestRunspaceWithPipeline()
        {
            using (Runspace runspace = RunspaceFactory.CreateRunspace())
            {
                runspace.Open();

                using (var pipeline = runspace.CreatePipeline(script))
                {
                    int objCount = 0;
                    foreach (var result in pipeline.Invoke())
                    {
                        ++objCount;
                        Assert.NotNull(result);
                    }
                    Assert.Equal(count, objCount);
                }

                runspace.Close();
            }
        }

        [Fact]
        public void TestRunspaceWithPowerShell()
        {
            using (var runspace = RunspaceFactory.CreateRunspace())
            {
                runspace.Open();

                using (PowerShell powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = runspace;

                    powerShell.AddScript(script);

                    int objCount = 0;
                    foreach (var result in powerShell.Invoke())
                    {
                        ++objCount;
                        Assert.NotNull(result);
                    }
                    Assert.Equal(count, objCount);
                }

                runspace.Close();
            }
        }


        [Fact]
        public void TestRunspaceWithPowerShellAndInitialSessionState()
        {
            InitialSessionState iss = InitialSessionState.CreateDefault2();

            // NOTE: instantiate custom host myHost for the next line to capture stdout and stderr output
            //       in addition to just the PSObjects
            using (Runspace runspace = RunspaceFactory.CreateRunspace(/*myHost,*/iss))
            {
                runspace.Open();
                using (PowerShell powerShell = PowerShell.Create())
                {
                    powerShell.Runspace = runspace;
                    powerShell.AddScript("Import-Module Microsoft.PowerShell.Utility -Force");
                    powerShell.AddScript(script);

                    int objCount = 0;

                    var results = powerShell.Invoke();

                    foreach (var result in results)
                    {
                        // this is how an object would be captured here and looked at,
                        // each result is a PSObject with the data from the pipeline
                        ++objCount;
                        Assert.NotNull(result);
                    }
                    Assert.Equal(count, objCount);
                    powerShell.Dispose();
                }
            }
        }
    }
}
