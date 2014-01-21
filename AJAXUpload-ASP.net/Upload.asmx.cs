using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Xml;

namespace AJAXUpload
{
    /// <summary>
    /// Summary description for Upload.
    /// </summary>
    public class Upload : System.Web.Services.WebService
    {
        public Upload()
        {
            //CODEGEN: This call is required by the ASP.NET Web Services Designer
            InitializeComponent();
        }
        const int InitialBufferLength = 8192;

        #region Component Designer generated code

        //Required by the Web Services Designer 
        private IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        [WebMethod]
        public XmlDocument UploadData(string fileName, int fileSize, byte[] file)
        {
            if (fileName == null || fileName == string.Empty || file == null)
                return GetXmlDocument(Guid.NewGuid(), "Incorrect UploadData Request", 0, 0);

            string filePath = GetFilePath(fileName);

            long offset = 0;
            using (FileStream fs = File.Open(filePath, FileMode.Append))
            {
                fs.Write(file, 0, file.Length);
                offset = fs.Length;
            }
            return GetXmlDocument(Guid.NewGuid(), string.Empty, offset, (InitialBufferLength + offset) > fileSize ? (int)(fileSize - offset) : InitialBufferLength);
        }

        [WebMethod]
        public XmlDocument InitUpload(int fileSize, string fileName, bool overwriteFile)
        {
            long offset = 0;
            string filePath = GetFilePath(fileName);

            if (File.Exists(filePath))
            {
                if (overwriteFile)
                {
                    File.Delete(filePath);
                }
                else
                {
                    using (FileStream fs = File.Open(filePath, FileMode.Append))
                    {
                        offset = fs.Length;
                    }
                }
            }

            return GetXmlDocument(Guid.NewGuid(), string.Empty, offset, (InitialBufferLength + offset) > fileSize ? (int)(fileSize - offset) : InitialBufferLength);
        }

        private string GetFilePath(string fileName)
        {
            string uploadFolder = Path.Combine(this.Context.ApplicationInstance.Request.PhysicalApplicationPath, "Upload");
            return Path.Combine(uploadFolder, fileName);
        }
        private XmlDocument GetXmlDocument(Guid id, string errorMessage, long offset, int bufferLength)
        {
            XmlDocument xml = new XmlDocument();
            MemoryStream memoryStream = new MemoryStream();

            XmlTextWriter writer = new XmlTextWriter(memoryStream, Encoding.UTF8);
            writer.WriteStartElement("UploadResponse");
            writer.WriteElementString("ID", id.ToString());
            writer.WriteElementString("ErrorMessage", errorMessage);
            writer.WriteElementString("OffSet", offset.ToString());
            writer.WriteElementString("BufferLength", bufferLength.ToString());
            writer.WriteEndElement();
            writer.Flush();

            writer.BaseStream.Position = 0;
            xml.Load(writer.BaseStream);
            writer.Close();

            return xml;
        }
    }
}
