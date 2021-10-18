using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DeversionatoreGISTable
{
    class Program
    {
        private static LicenseInitializer m_AOLicenseInitializer = new DeversionatoreGISTable.LicenseInitializer();


        /// <summary>
        /// Registra come versionato un oggetto
        /// </summary>
        /// <param name="dataset"></param>
        public static void RegistraComeVersionato(IDataset dataset, bool editaInBaseTable=true)
        {
            IVersionedObject3 versionedObject = (IVersionedObject3)dataset;

            versionedObject.GetVersionRegistrationInfo(out bool IsRegistered, out bool IsMovingEditsToBase);

            if (IsRegistered)
            {
                if (IsMovingEditsToBase)   
                    throw new ArgumentException("La tabella in input è già versionata!");
            }
            else
            {
                versionedObject.RegisterAsVersioned3(editaInBaseTable);
            }
        }

        /// <summary>
        /// Deregistra come non versionato.
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="comprimiSuDefault"></param>
        public static void DeRegistraComeVersionato(IDataset dataset, bool comprimiSuDefault = true)
        {
            IVersionedObject3 versionedObject = (IVersionedObject3)dataset;

            versionedObject.GetVersionRegistrationInfo(out bool IsRegistered, out bool IsMovingEditsToBase);

            if (IsRegistered)
            {
                versionedObject.UnRegisterAsVersioned3(comprimiSuDefault);
            }
            else
                throw new ArgumentException("La fc/tabella scelta non è versionata!");
        }

        [STAThread()]
        static void Main(string[] args)
        {
            //ESRI License Initializer generated code.
            m_AOLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeStandard },
            new esriLicenseExtensionCode[] { });
            //ESRI License Initializer generated code.

            try
            {
                IWorkspaceFactory workF = new SdeWorkspaceFactory();
                IPropertySet2 propertySetSDE = new PropertySetClass(); // Imposto le proprietà di connessione al SDE Workspace Factory
                propertySetSDE.SetProperty("DATABASE", "SDE");
                propertySetSDE.SetProperty("INSTANCE", string.Format("sde:oracle11g:{0}", DeversionatoreGISTable.Properties.Settings.Default.Istanza));
                propertySetSDE.SetProperty("SERVER", DeversionatoreGISTable.Properties.Settings.Default.Istanza);
                propertySetSDE.SetProperty("User", DeversionatoreGISTable.Properties.Settings.Default.User);
                propertySetSDE.SetProperty("Password", DeversionatoreGISTable.Properties.Settings.Default.Password);
                propertySetSDE.SetProperty("VERSION", "SDE.DEFAULT");

                IWorkspace workspace = workF.Open(propertySetSDE, 0);
                IFeatureWorkspace fWs = (IFeatureWorkspace)workspace;
                List<string> listaNomiFc = DeversionatoreGISTable.Properties.Settings.Default.FeatureClassDaDeversionare.Split(';').ToList<string>();
                List<IFeatureClass> listaFc = new List<IFeatureClass>();
                if (listaNomiFc.Count > 0)
                    listaNomiFc.Where(nomeFc => nomeFc != null && !string.IsNullOrEmpty(nomeFc)).ToList().ForEach(nomeFc => listaFc.Add(fWs.OpenFeatureClass(nomeFc)));

                List<string> listaNomiTabelle = DeversionatoreGISTable.Properties.Settings.Default.TabelleGISDaDeversionare.Split(';').ToList<string>();
                List<ITable> listaTabelle = new List<ITable>();
                if (listaNomiTabelle.Count > 0)
                    listaNomiTabelle.Where(nomeTab => nomeTab != null && !string.IsNullOrEmpty(nomeTab)).ToList().ForEach(nomeTab => listaTabelle.Add(fWs.OpenTable(nomeTab)));

                // Procedo nel deversionamento
                listaFc.ForEach(featureClass =>
                    {
                        try
                        {
                            DeRegistraComeVersionato((IDataset)featureClass, true);
                        }
                        catch (ArgumentException ex) // Intercetto except custom mio metodo
                        {
                            Console.WriteLine(ex.Message);
                            var wait = Console.ReadLine();
                        }
                        catch (Exception)
                        {
                            throw;
                        }             
                    });

                listaTabelle.ForEach(table =>
                    {
                        try
                        {
                            DeRegistraComeVersionato((IDataset)table, true);
                        }
                        catch (ArgumentException ex)
                        {
                            Console.WriteLine(ex.Message);
                            var wait = Console.ReadLine();
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }


            //Do not make any call to ArcObjects after ShutDownApplication()
            m_AOLicenseInitializer.ShutdownApplication();
        }
    }
}
