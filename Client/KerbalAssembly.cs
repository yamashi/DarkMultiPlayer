using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DarkMultiPlayer
{
    public class KerbalAssembly
    {
        private string m_assemblyPath;
        private string m_assemblyValidPath;
        private string m_kspPath;
        private bool m_isValid;

        public string AssemblyPath
        {
            get { return m_assemblyPath; }
        }

        public string AssemblyValidPath
        {
            get { return m_assemblyValidPath; }
        }

        public string KspPath
        {
            get { return m_kspPath; }
        }
        public bool IsValid
        {
            get { return m_isValid; }
        }

        public KerbalAssembly()
        {
            m_assemblyPath = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).FullName;
            m_kspPath = new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName;
            //I find my abuse of Path.Combine distrubing.
            m_assemblyValidPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(KspPath, "GameData"), "DarkMultiPlayer"), "Plugins"), "DarkMultiPlayer.dll");

            m_isValid = AssemblyPath.ToLower() == AssemblyValidPath.ToLower();
        }
    }
}
