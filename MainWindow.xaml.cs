using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ADP_
{
    public partial class MainWindow : Window
    {
        public Dictionary<int, string> Function_IDs = new()
        {
            {0, "Nothing (?)"},
            {1, "Overall Brightness"},
            {2, "Character Light (RED)"},
            {3, "Character Light (GREEN)"},
            {4, "Character Light (BLUE)"},
            {6, "Character Colour (RED)"},
            {7, "Character Colour (GREEN)"},
            {8, "Character Colour (BLUE)"},
            {10, "ITEM and EYE Specular (RED)"},
            {11, "ITEM and EYE Specular (GREEN)"},
            {12, "ITEM and EYE Specular (BLUE)"},
            {14, "CLOTH Specular"},
            {15, "Character Lighting"},
            {17, "Generic Lighting"},
            {18, "Skin Shine"},
            {21, "Toon Shader Thickness"},
            {22, "Toon Shader Strength (Eyelid)"},
            {37, "Toon Shader Strength (Body)"},
            {38, "Toon Shader Strength (Cloth/Hair)"},
            {39, "EYE Ambient Lighting"},
            {40, "Toon Shader Strength (Skin)"},
            {41, "HAIR Diffuse Lighting"},
            {42, "Cloth Lighting"},
            {43, "Item/Facial Lighting"},
            {44, "HAIR Ambient Lighting"},
            {46, "Stage Lighting"},
            {57, "Hair Shininess"},
            {59, "Character Warmth (Orange Tint)"},
            {60, "Character Lightness"},
            {61, "Subsurface Scattering (SSS)"},
            {62, "Skin Lightness"},
            {72, "Eye Colour Saturation"},
            {75, "Handheld 30FPS Limit [Switch]"},
            {76, "Docked 30FPS Limit [Switch]"},
            {77, "Handheld Resolution [Switch]"},
            {78, "Docked Resolution [Switch]"},
        };

        public MainWindow()
        {
            InitializeComponent();
        }
        public string current_path;
        public static ADP main_adp;
        public static short fps1 = 1;
        public static short fps2 = 16224;


        public void Read_ADP(string path)
        {
            main_adp = new ADP() { functions = [], header = new Header() };
            BinaryReader br = new BinaryReader(File.OpenRead(path));
            try
            {
                using (br) //read for a var with the adpfunc values to use for funcDetect
                {
                    main_adp.header.function_count = br.ReadInt64();
                    main_adp.header.data_length = br.ReadInt64();
                    main_adp.header.offset = br.ReadInt64(); // always 24?
                    br.BaseStream.Position = 0x18;
                    Debug.WriteLine(main_adp.header.function_count);
                    Debug.WriteLine(main_adp.header.data_length);
                    for (var i = 0; i < main_adp.header.function_count; i++)
                    {
                        var temp_function = new Function { time_seconds = br.ReadSingle() };
                        br.ReadBytes(4);
                        temp_function.time_frames = br.ReadInt32();
                        br.ReadBytes(4);
                        temp_function.alt_pv_flag = br.ReadBoolean();
                        br.ReadBytes(3);
                        temp_function.ID = br.ReadInt32();
                        if(temp_function.ID == 75 || temp_function.ID == 76)
                        {
                            temp_function.is_30_fps = br.ReadBoolean();
                            br.ReadBytes(3);
                            temp_function.Value = -1;
                        }
                        else
                        {
                            temp_function.Value = br.ReadSingle();
                        }
                        br.ReadBytes(4);
                        main_adp.functions.Add(temp_function);
                    }
                }
                br.Close();
                Debug.WriteLine("Read ADP without issue.");
            }
            catch (Exception)
            {
                Debug.WriteLine("An error occurred whilst reading the ADP.");
            }
        }

        public void saveFile(string path)
        {
            if (main_adp != null)
            {
                main_adp.header.function_count = main_adp.functions.Count;
                main_adp.header.data_length = main_adp.header.function_count * 32;
                using (BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.Create)))
                {
                    bw.Write(main_adp.header.function_count);
                    bw.Write(main_adp.header.data_length);
                    bw.Write(main_adp.header.offset);
                    foreach (Function f in main_adp.functions)
                    {

                        bw.Write(f.time_seconds);
                        bw.Write(0);
                        bw.Write(f.time_frames);
                        bw.Write(0);
                        bw.Write(f.alt_pv_flag);
                        bw.Write(new byte[] { 0x00, 0x00, 0x00 });
                        bw.Write(f.ID);
                        if (f.is_30_fps && (f.ID == 75 || f.ID == 76))
                        {
                            bw.Write(fps1);
                            bw.Write(fps2);
                        }
                        else
                        {
                            bw.Write(f.Value);
                        }
                        bw.Write(0);
                    }
                    bw.Close();
                }
            }
        }

        private void Open_ADP()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "ADP Files|*.adp", Title = "Please select an ADP file" };
            if (ofd.ShowDialog() == true)
            {
                Read_ADP(ofd.FileName);
                current_path = ofd.FileName;
                Update_Function_Names();
                D_tree.DataContext = main_adp;
                //D_tree.ItemsSource = main_adp.functions;
                D_tree.DisplayMemberPath = "name";
            }
        }

        private void Update_Function_Names()
        {
            foreach (Function f in main_adp.functions)
            {
                string name;
                if (!Function_IDs.TryGetValue(f.ID, out name))
                {
                    name = "Unknown Function: ID: " + f.ID;
                }
                f.name = "[" + f.time_frames + "," + f.alt_pv_flag.ToString() + "] " + name;
            }
            //D_tree.DataContext = main_adp;
            //D_tree.Items.Refresh();
            D_count_text.Header = "Function Count: " + main_adp.functions.Count;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Open_ADP();
        }

        public void Save_As_ADP()
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog() { Filter = "ADP files (*.adp)|*.adp", Title="Please save your ADP file" };
                sfd.ShowDialog();
                saveFile(sfd.FileName);
            }
            catch (Exception) { Debug.WriteLine("Error during Save As process."); }
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Open_ADP();
        }
        private void SaveCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(current_path != null)
            {
                Save_ADP();
            }
            else
            {

            }
        }
        private void SaveAsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save_As_ADP();
        }

        public void Save_ADP()
        {
            saveFile(current_path);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save_ADP();
        }

        private void Save_As_Click(object sender, RoutedEventArgs e)
        {
            Save_As_ADP();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if(main_adp != null)
            {
                Function new_f = new()
                {
                    time_frames = 0,
                    time_seconds = 0,
                    Value = 1,
                    ID = 15,
                    is_30_fps = false,
                    alt_pv_flag = false,
                };
                main_adp.functions.Add(new_f);
                Update_Function_Names();
            }
        }
        private void Del_Click(object sender, RoutedEventArgs e)
        {
            if (main_adp != null)
            {
                main_adp.functions.Remove(D_tree.SelectedItem as Function);
                Update_Function_Names();
            }
        }

        private void D_tree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Update_Function_Names();
            D_tree.Items.Refresh();
            D_grid.DataContext = D_tree.SelectedItem as Function;
            var func = D_grid.DataContext as Function;
            if(func != null)
            {
                if (func.ID != 75 && func.ID != 76)
                {
                    D_check_2.IsEnabled = false;
                }
                else
                {
                    D_check.IsEnabled = true;
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is jaykthx! Thank you for using my tool, MegaADP!" +
                "\nThis tool is an updated version of ADPEdit." +
                "\nThank you to CyberKevin for maintaining ADPEdit, researching the ADP format further and fixing my errors." +
                "\nThank you to korenkonder for researching the ADP header." +
                "\nThank you to BroGamer for initial coding advice when I first worked on ADPEdit.");
        }

        private void D_id_TextChanged(object sender, TextChangedEventArgs e)
        {
           Function f = D_tree.SelectedItem as Function;
            if (f != null)
            {
                if(f.ID == 75 || f.ID == 76)
                {
                    D_check_2.IsEnabled = true;
                    Debug.WriteLine("enabled checkbox");
                }
                else
                {
                    D_check_2.IsEnabled = false;
                    f.is_30_fps = false;
                    Debug.WriteLine("disabled checkbox and set 30fps to false");
                }
            }
        }
    }
}