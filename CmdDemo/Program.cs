using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TargetInterface;
using DeviceCommunication;
using Utilities.Feedback;
using System.ComponentModel;

namespace CmdDemo
{
    public class CmdDemoNotifier : FeedbackConsoleNotifier
    {
        public CmdDemoNotifier(string prefix = "") : base (prefix)
        {

        }

        public override void Feedback_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // base.Feedback_PropertyChanged(sender, e);

            switch (e.PropertyName)
            {
                case "FeedbackOfActions":
                    {
                        FeedbackPropertyChange feedback = (FeedbackPropertyChange)sender;

                        switch (feedback.FeedbackOfActions.FeedbackType)
                        {
                            case FeedBackType.Error:
                                System.Console.WriteLine(this.Prefix + feedback.FeedbackOfActions.FeedbackMessage);
                                break;
                            case FeedBackType.Info:
                                System.Console.WriteLine(this.Prefix + feedback.FeedbackOfActions.FeedbackMessage);
                                break;
                            case FeedBackType.InfoVerbose:
                                //System.Console.WriteLine(this.Prefix + feedback.FeedbackOfActions.FeedbackMessage);
                                break;
                            case FeedBackType.Warning:
                                System.Console.WriteLine(this.Prefix + feedback.FeedbackOfActions.FeedbackMessage);
                                break;
                        }
                        
                        break;
                    }
                case "LinkStatus":
                    {
                        FirmwareAPI fwAPI = (FirmwareAPI)sender;
                        System.Console.WriteLine(">>>> " + this.Prefix + " Link Change {0:s}", fwAPI.LinkStatus.ToString());
                    }                    
                    break;

            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Welcome to the show.");
            List<CmdDemoNotifier> consoleNotifiers = new List<CmdDemoNotifier>();
            List<DeviceConnection> deviceConnections = new List<DeviceConnection>();
            List<FirmwareAPI> fwAPIs = new List<FirmwareAPI>();
            DeviceConnection.RefreshConnectedDevices();
            int numDevices = (int)DeviceConnection.DeviceSerialNumbers.Count;
            try
            {
                for (int deviceIndex = 0; deviceIndex < numDevices; deviceIndex++)
                {
                    CmdDemoNotifier consoleNotifier = new CmdDemoNotifier(string.Format("<Device {0:d}> ", deviceIndex));
                    DeviceConnection deviceConnection = new DeviceConnection(DeviceConnection.DeviceSerialNumbers[deviceIndex]);
                    FirmwareAPI fwAPI = new FirmwareAPI();

                    fwAPI.PropertyChanged += consoleNotifier.Feedback_PropertyChanged;
                    deviceConnection.PropertyChanged += consoleNotifier.Feedback_PropertyChanged;
                    consoleNotifiers.Add(consoleNotifier);

                    deviceConnection.Open();
                    fwAPI.AttachDevice(deviceConnection);

                    fwAPIs.Add(fwAPI);
                }

                try
                {

                    while (true)

                    {
                        /* It seems a software reset is needed.*/
                        System.Console.WriteLine("Software Reset all connected PHYS");
                        foreach (FirmwareAPI fwAPI in fwAPIs)
                        {
                            fwAPI.SftRstGEPhy();
                        }

                        if (numDevices == 2)
                        {
                            fwAPIs[0].ReportPhyStatus();
                            fwAPIs[1].ReportPhyStatus();

                            foreach (FirmwareAPI.EthForcedSpeed forced_speed in Enum.GetValues(typeof(FirmwareAPI.EthForcedSpeed)))
                            {
                                System.Console.WriteLine(string.Format("force speed to {0:s}", forced_speed.ToString()));
                                fwAPIs[0].ForcedSpeedSelect(forced_speed);
                                fwAPIs[0].ReportPhyStatus();
                                fwAPIs[1].ReportPhyStatus();
                                fwAPIs[0].RestartANeg();
                                while (fwAPIs[0].ReadYodaRg("GEPhy", "LinkStatLat") == 0)
                                {
                                    System.Console.WriteLine(string.Format("Waiting for link to be established for {0:s}", forced_speed.ToString()));
                                    fwAPIs[0].ReportPhyStatus();
                                    fwAPIs[1].ReportPhyStatus();
                                }
                                System.Console.WriteLine("Link established!");
                                fwAPIs[0].ReportPhyStatus();
                                fwAPIs[1].ReportPhyStatus();
                            }
                        }

                        foreach (FirmwareAPI fwAPI in fwAPIs)
                        {
                            fwAPI.ANegEnable(true);
                            fwAPI.RestartANeg();
                        }
                    }
                }

                catch (System.ArgumentException e)
                {
                    throw;
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                foreach (DeviceConnection deviceConnection in deviceConnections)
                {
                    deviceConnection.Close();
                }
            }
        }
    }
}
