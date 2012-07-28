using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;

namespace LocationClasses
{
    public class CodeBuilder
    {
        ObservableCollection<CodeSnippet> m_CodeSnippets = null;

        public ObservableCollection<CodeSnippet> CodeSnippets
        {
            get
            {
                if (m_CodeSnippets == null)
                    m_CodeSnippets = GetAllCodeSnippets();
                return m_CodeSnippets;
            }
            set { m_CodeSnippets = value; }
        }

        public CodeSnippet LoadFromFile(string strRelativePath)
        {
            CodeSnippet item = new CodeSnippet();
            item.Name = "Google Maps Source Code";
            item.ApiName = "Google Maps";
            if (strRelativePath == "")
                item.RelativePath = "Code/BrowserWindowSource.html";
            else
                item.RelativePath = strRelativePath;
            try
            {
                //if (System.IO.File.Exists(item.RelativePath))
                {
                    //StreamReader file = File.OpenText(item.RelativePath);
                    StreamReader file = File.OpenText(item.RelativePath);
                    //Uri uri = null;
                    //uri = new Uri(item.RelativePath, UriKind.Relative);

                    // Read the file into a string
                    string s = file.ReadToEnd();
                    item.Body = s;
                    item.Html = s;
                }
                
            }
            catch (Exception e)
            {

            }
            return item;
        }

        public ObservableCollection<CodeSnippet> GetAllCodeSnippets()
        {
            ObservableCollection<CodeSnippet> retList = new ObservableCollection<CodeSnippet>();
          
            CodeSnippet snippet = new CodeSnippet()
            {
                ApiName = "Google Earth",
                Name = "Hello, Earth",
                SourceURL = "http://code.google.com/apis/ajax/playground/#hello,_earth",
                LocalFileName = @"CodeSnippets\Google Earth\Hello, Earth.html"
                
            };




            // TODO: Load from local file.
            snippet.HtmlBody = @"
 <body onload=""init()"" style=""font-family: arial, sans-serif; font-size: 13px; border: 0;"">
    <div id=""map3d"" style=""width: 500px; height: 380px;""></div>
    <br>
    <div>Installed Plugin Version: <span id=""installed-plugin-version"" style=""font-weight: bold;"">Loading...</span></div>
  </body>
";

            snippet.HtmlHead = @"
  <head>
    <meta http-equiv=""content-type"" content=""text/html; charset=utf-8""/>
    <title>Google Earth API Sample</title>
    <script src=""http://www.google.com/jsapi?key=ABQIAAAAuPsJpk3MBtDpJ4G8cqBnjRRaGTYH6UMl8mADNa0YKuWNNa8VNxQCzVBXTx2DYyXGsTOxpWhvIG7Djw"" type=""text/javascript""></script>
    <script type=""text/javascript"">
    var ge;
    
    google.load(""earth"", ""1"");
    
    function init() {
      google.earth.createInstance('map3d', initCallback, failureCallback);
    }
    
    function initCallback(pluginInstance) {
      ge = pluginInstance;
      ge.getWindow().setVisibility(true);
    
      // add a navigation control
      ge.getNavigationControl().setVisibility(ge.VISIBILITY_AUTO);
    
      // add some layers
      ge.getLayerRoot().enableLayerById(ge.LAYER_BORDERS, true);
      ge.getLayerRoot().enableLayerById(ge.LAYER_ROADS, true);
    
      // just for debugging purposes
      document.getElementById('installed-plugin-version').innerHTML =
        ge.getPluginVersion().toString();
    }
    
    function failureCallback(errorCode) {
    }
    
    </script>
  </head>
";


            snippet.Body = @"
var ge;

google.load(""earth"", ""1"");

function init() {
  google.earth.createInstance('map3d', initCallback, failureCallback);
}

function initCallback(pluginInstance) {
  ge = pluginInstance;
  ge.getWindow().setVisibility(true);

  // add a navigation control
  ge.getNavigationControl().setVisibility(ge.VISIBILITY_AUTO);

  // add some layers
  ge.getLayerRoot().enableLayerById(ge.LAYER_BORDERS, true);
  ge.getLayerRoot().enableLayerById(ge.LAYER_ROADS, true);

  // just for debugging purposes
  document.getElementById('installed-plugin-version').innerHTML =
    ge.getPluginVersion().toString();
}

function failureCallback(errorCode) {
}
​";

            snippet.BuildHtml();
            retList.Add(snippet);

            return retList;
        }
    }
}