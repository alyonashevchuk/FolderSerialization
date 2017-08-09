using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace GlobalLogicTestTask
{
    public partial class Form1 : Form
    {
        private string folderName, unpackDir;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                folderName = folderBrowserDialog1.SelectedPath;
                textBox1.Text = folderName;
            }
        }

        private void DirectoryTree(DirectoryInfo root)
        {
            try
            {
                // Process all the files directly under this folder
                foreach (var fi in root.GetFiles("*.*"))
                {
                    Structure.FileStructure file = new Structure.FileStructure();

                    file.FileName = fi.FullName;
                    file.Content = File.ReadAllBytes(fi.FullName);
                }

                // Process all the subdirectories under this directory
                foreach (var dir in root.GetDirectories())
                {
                    Structure.FolderStructure folder = new Structure.FolderStructure();
                    folder.FolderName = dir.FullName;

                    // Resursive call for each subdirectory
                    DirectoryTree(dir);
                }
            }

            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("UnAuthorizedAccessException: Unable to access file. ");
            }

            catch (DirectoryNotFoundException e)
            {
                MessageBox.Show("Directory not found: " + e.Message);
            }
        }

        // Binary serialization
        static void SaveBinaryFormat(object objGraph, string fileName)
        {
            BinaryFormatter binFormat = new BinaryFormatter();

            using (Stream fStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                binFormat.Serialize(fStream, objGraph);
            }

            MessageBox.Show("The operation completed successfully. ");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DirectoryInfo rootDir = new DirectoryInfo(folderName);
            DirectoryTree(rootDir);
            
            SaveBinaryFormat(rootDir, "Folder.dat");
        }

        // Binary deserialization
        static DirectoryInfo LoadFromBinaryFile(string fileName)
        {
            DirectoryInfo rootDir = null;

            try
            {
                BinaryFormatter binFormat = new BinaryFormatter();

                using (Stream fStream = File.OpenRead(fileName))
                {
                    rootDir = (DirectoryInfo)binFormat.Deserialize(fStream);
                }

                MessageBox.Show("The operation completed successfully. ");
            }

            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show("Failed to deserialize. Reason: " + e.Message);
            }

            return rootDir;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DirectoryInfo rootDir = LoadFromBinaryFile("Folder.dat");

            DirectoryInfo resultDir = Directory.CreateDirectory(unpackDir);

            CopyFilesRecursively(rootDir, resultDir);
        }

        // Select folder to deserialize
        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                unpackDir = folderBrowserDialog1.SelectedPath;
                textBox2.Text = unpackDir;
            }
        }

        // A directory copy using recursion
        private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));

            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }
    }
}
