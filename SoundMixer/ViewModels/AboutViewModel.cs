using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

using Stylet;

namespace SoundMixer.ViewModels
{
    class AboutViewModel : Screen
    {
        readonly private string programName;
        readonly private string programVersion;
        readonly private DateTime buildDate;
        
        readonly private string authorName;
        readonly private string projectWebsite;


        readonly private string licenceText = "MIT License" +
            "\n\n" +
            "Copyright (c) 2020 Juan Ferrer" +
            "\n\n" +
            "Permission is hereby granted, free of charge, to any person obtaining a copy " +
            "of this software and associated documentation files (the \"Software\"), to deal " +
            "in the Software without restriction, including without limitation the rights " +
            "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell " +
            "copies of the Software, and to permit persons to whom the Software is " +
            "furnished to do so, subject to the following conditions:" +
            "\n\n" +
            "The above copyright notice and this permission notice shall be included in all " +
            "copies or substantial portions of the Software." +
            "\n\n" +
            "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR " +
            "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, " +
            "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE " +
            "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER " +
            "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, " +
            "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE " +
            "SOFTWARE.";



        private string buildInfo;
        public string BuildInfo
        {
            get { return this.buildInfo; }
            set { this.SetAndNotify(ref this.buildInfo, value); }
        }

        private string authorInfo;
        public string AuthorInfo
        {
            get { return this.authorInfo; }
            set { this.SetAndNotify(ref this.authorInfo, value); }
        }

        private string websiteInfo;
        public string WebsiteInfo
        {
            get { return this.websiteInfo; }
            set { this.SetAndNotify(ref this.websiteInfo, value); }
        }

        private string licenceInfo;
        public string LicenceInfo
        {
            get { return this.licenceInfo; }
            set { this.SetAndNotify(ref this.licenceInfo, value); }
        }

        private DateTime GetBuildDate()
        {
            var attr = Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyMetadataAttribute), false) as AssemblyMetadataAttribute;
            return DateTime.Parse(attr?.Value);
        }

        public AboutViewModel()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(executingAssembly.Location);
            
            programName = fvi.ProductName;
            programVersion = fvi.ProductVersion;
            // buildDate = GetBuildDate();

            authorName = fvi.CompanyName;
            projectWebsite = "https://github.com/juanferrer/sound-mixer";

            BuildInfo = $"{programName} {programVersion}";
            AuthorInfo = authorName;
            WebsiteInfo = projectWebsite;
            LicenceInfo = licenceText;
        }

        public void Hyperlink_RequestNavigate(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo()
            {
                FileName = (e as RequestNavigateEventArgs)?.Uri.ToString(),
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
