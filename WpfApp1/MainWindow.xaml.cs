using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using WpfApp1.Model;
using Brushes = System.Drawing.Brushes;
using Pen = System.Drawing.Pen;
using Point = WpfApp1.Model.Point;
using Size = System.Drawing.Size;


//NewValue = (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class Node
    {
        int vr;
        int i, j;
        Node previous;

        public Node(int i, int j)
        {
            Vr = 0;
            I = i;
            J = j;
        }


        public int Vr { get => vr; set => vr = value; }
        public Node Previous { get => previous; set => previous = value; }
        public int I { get => i; set => i = value; }
        public int J { get => j; set => j = value; }
    }
    public partial class MainWindow : Window
    {
        List<System.Windows.Point> preseci = new List<System.Windows.Point>();

        public double noviX, noviY;
        List<double> temp_pointsX;
        List<PowerEntity> listasvega;
        List<UIElement> uielements = new List<UIElement>();
        List<System.Windows.Media.Brush> brushes = new List<System.Windows.Media.Brush>();
        List<LineEntity> linentitylist;
        List<Polyline> polylinije;
        List<SubstationEntity> substationlist;
        List<NodeEntity> nodesentity = new List<NodeEntity>();
        List<SwitchEntity> switchlist;
        List<Tuple<Point, Point>> linije = new List<Tuple<Point, Point>>();
        List<double> temp_pointsY;
        int[,] matrix;

        public int n = 100;
        public int m = 100;

        int[] dx = { 1, 0, -1, 0 };
        int[] dy = { 0, 1, 0, -1 };

        double subminX;
        double submaxX;
        double subminY;
        double submaxY;

        double swminX;
        double swmaxX;
        double swminY;
        double swmaxY;

        double nodeminX;
        double nodemaxX;
        double nodeminY;
        double nodemaxY;

        double leminX;
        double lemaxX;
        double leminY;
        double lemaxY;

        GMapControl gmap = new GMapControl();

        public List<DependencyObject> hitResults = new List<DependencyObject>();




        public MainWindow()
        {
            matrix = new int[100, 100];

            //for (int i = 0; i < 3; i++)
            //{
            //    for (int j = 0; j < 3; j++)
            //    {
            //        Trace.Write(matrix[i, j] + " ");
            //    }
            //    Trace.WriteLine("");//new line at each row  
            //}

            InitializeComponent();

            polylinije = new List<Polyline>();
            listasvega = new List<PowerEntity>();
            substationlist = new List<SubstationEntity>();
            switchlist = new List<SwitchEntity>();
            linentitylist = new List<LineEntity>();
            temp_pointsX = new List<double>();
            temp_pointsY = new List<double>();

            GMapProvider.WebProxy = WebRequest.GetSystemWebProxy();
            GMapProvider.WebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            gmap.MapProvider = GMap.NET.MapProviders.BingMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            //gmap.SetPositionByKeywords("Novi Sad, Serbia");
            gmap.Position = new GMap.NET.PointLatLng(45.254765, 19.844184);
            gmap.ShowCenter = false;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {

            this.ResizeMode = ResizeMode.CanResize;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");

            XmlNodeList nodeList;

            GMap.NET.WindowsForms.GMapOverlay subsOverlay = new GMap.NET.WindowsForms.GMapOverlay("SubstationsOverlay");


            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {

                SubstationEntity sub = new SubstationEntity();

                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                ToLatLon(sub.X, sub.Y, 34, out noviX, out noviY);
                Point p = new Point();
                sub.X = noviX;
                sub.Y = noviY;
                p.X = noviX;
                p.Y = noviY;

                temp_pointsX.Add(p.X);
                temp_pointsY.Add(p.Y);

                substationlist.Add(sub);
                listasvega.Add(sub);

            }


            subminX = temp_pointsX.Min();
            submaxX = temp_pointsX.Max();
            subminY = temp_pointsY.Min();
            submaxY = temp_pointsY.Max();

            foreach (SubstationEntity sub in substationlist)
            {
                AddToCanvas(subminX, subminY, submaxX, submaxY, 1, 1, System.Windows.Media.Brushes.Black, sub);
            }

            temp_pointsX.Clear();
            temp_pointsY.Clear();


            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode node in nodeList)
            {

                NodeEntity sub = new NodeEntity();

                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                ToLatLon(sub.X, sub.Y, 34, out noviX, out noviY);
                Point p = new Point();
                sub.X = noviX;
                sub.Y = noviY;
                p.X = noviX;
                p.Y = noviY;

                temp_pointsX.Add(p.X);
                temp_pointsY.Add(p.Y);

                nodesentity.Add(sub);
                listasvega.Add(sub);

            }

            nodeminX = temp_pointsX.Min();
            nodemaxX = temp_pointsX.Max();
            nodeminY = temp_pointsY.Min();
            nodemaxY = temp_pointsY.Max();


            foreach (NodeEntity sub in nodesentity)
            {
                AddToCanvas(nodeminX, nodeminY, nodemaxX, nodemaxY, 0.6, 0.6, System.Windows.Media.Brushes.Purple, sub);
            }

            temp_pointsX.Clear();
            temp_pointsY.Clear();


            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode node in nodeList)
            {
                SwitchEntity sw = new SwitchEntity();

                sw.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sw.Name = node.SelectSingleNode("Name").InnerText;
                sw.Status = node.SelectSingleNode("Status").InnerText;
                sw.X = double.Parse(node.SelectSingleNode("X").InnerText);
                sw.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                ToLatLon(sw.X, sw.Y, 34, out noviX, out noviY);
                sw.X = noviX;
                sw.Y = noviY;
                Point p = new Point();
                p.X = noviX;
                p.Y = noviY;

                temp_pointsX.Add(p.X);
                temp_pointsY.Add(p.Y);
                switchlist.Add(sw);
                listasvega.Add(sw);
            }

            swminX = temp_pointsX.Min();
            swmaxX = temp_pointsX.Max();
            swminY = temp_pointsY.Min();
            swmaxY = temp_pointsY.Max();

            foreach (SwitchEntity switchh in switchlist)
            {
                AddToCanvas(swminX, swminY, swmaxX, swmaxY, 0.6, 0.6, System.Windows.Media.Brushes.Red, switchh);
            }

            temp_pointsX.Clear();
            temp_pointsY.Clear();


            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode node in nodeList)
            {
                LineEntity l = new LineEntity();
                l.Vertices = new List<Point>();
                l.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                l.Name = node.SelectSingleNode("Name").InnerText;
                if (node.SelectSingleNode("IsUnderground").InnerText.Equals("true"))
                {
                    l.IsUnderground = true;
                }
                else
                {
                    l.IsUnderground = false;
                }
                l.R = float.Parse(node.SelectSingleNode("R").InnerText);
                l.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                l.LineType = node.SelectSingleNode("LineType").InnerText;
                l.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                l.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                l.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);

                linentitylist.Add(l);
            }



            IterateOverLines1();
            IterateOverLines2();
            IterateOverLines3();
            IterateOverLines4();
            IterateOverLines5();
            IterateOverLines6();
            IterateOverLines7();
            IterateOverLines8();
            IterateOverLines9();

            LoadButton.Click -= LoadButton_Click;
        }



        private void IterateOverLines2()
        {
            foreach (LineEntity le in linentitylist)
            {
                bool flag = false;
                long id1 = le.FirstEnd;
                long id2 = le.SecondEnd;
                SubstationEntity pe1 = substationlist.SingleOrDefault(x => x.Id == id1);
                SwitchEntity pe2 = switchlist.SingleOrDefault(x => x.Id == id2);

                if (pe1 != null && pe2 != null)
                {
                    Point s = new Point();
                    s.X = pe1.X;
                    s.Y = pe1.Y;
                    Point d = new Point();
                    d.X = pe2.X;
                    d.Y = pe2.Y;

                    foreach (Tuple<Point, Point> tup in linije)
                    {
                        if ((s.X == tup.Item1.X && s.Y == tup.Item1.Y) && (d.X == tup.Item2.X && d.Y == tup.Item2.Y))
                        {
                            flag = true;
                        }
                        else if ((s.X == tup.Item2.X && s.Y == tup.Item2.Y) && (d.X == tup.Item1.X && d.X == tup.Item1.Y))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {

                            Point p1 = new Point();
                            Point p2 = new Point();
                            p1.X = s.X;
                            p1.Y = s.Y;
                            p2.X = d.X;
                            p2.Y = d.Y;

                            linije.Add(new Tuple<Point, Point>(p1, p2));

                            leminX = subminX;
                            lemaxX = submaxX;
                            leminY = subminY;
                            lemaxY = submaxY;

                            s.X = Math.Round((((s.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            s.Y = Math.Round((((s.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);

                            leminX = swminX;
                            lemaxX = swmaxX;
                            leminY = swminY;
                            lemaxY = swmaxY;

                            d.X = Math.Round((((d.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            d.Y = Math.Round((((d.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            matrix[(int)d.X, (int)d.Y] = 6;

                            Paths(s, le);
                        
                    }
                }

            }
        }
        private void IterateOverLines3()
        {
            foreach (LineEntity le in linentitylist)
            {
                bool flag = false;
                long id1 = le.FirstEnd;
                long id2 = le.SecondEnd;
                SubstationEntity pe1 = substationlist.SingleOrDefault(x => x.Id == id1);
                NodeEntity pe2 = nodesentity.SingleOrDefault(x => x.Id == id2);

                if (pe1 != null && pe2 != null)
                {
                    Point s = new Point();
                    s.X = pe1.X;
                    s.Y = pe1.Y;
                    Point d = new Point();
                    d.X = pe2.X;
                    d.Y = pe2.Y;

                    foreach (Tuple<Point, Point> tup in linije)
                    {
                        if ((s.X == tup.Item1.X && s.Y == tup.Item1.Y) && (d.X == tup.Item2.X && d.Y == tup.Item2.Y))
                        {
                            flag = true;
                        }
                        else if ((s.X == tup.Item2.X && s.Y == tup.Item2.Y) && (d.X == tup.Item1.X && d.X == tup.Item1.Y))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {

                        if (substationlist.Any(x => x.X == s.X && x.Y == s.Y) && nodesentity.Any(x => x.Y == d.Y && x.Y == d.Y))
                        {
                            Point p1 = new Point();
                            Point p2 = new Point();
                            p1.X = s.X;
                            p1.Y = s.Y;
                            p2.X = d.X;
                            p2.Y = d.Y;

                            linije.Add(new Tuple<Point, Point>(p1, p2));

                            leminX = subminX;
                            lemaxX = submaxX;
                            leminY = subminY;
                            lemaxY = submaxY;

                            s.X = Math.Round((((s.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            s.Y = Math.Round((((s.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);

                            leminX = nodeminX;
                            lemaxX = nodemaxX;
                            leminY = nodeminY;
                            lemaxY = nodemaxY;

                            d.X = Math.Round((((d.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            d.Y = Math.Round((((d.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            matrix[(int)d.X, (int)d.Y] = 6;

                            Paths(s, le);

                            linentitylist.ToList().Remove(le);

                        }

                    }
                }

            }
        }
        private void IterateOverLines1()
        {
            foreach (LineEntity le in linentitylist)
            {
                bool flag = false;
                long id1 = le.FirstEnd;
                long id2 = le.SecondEnd;
                SubstationEntity pe1 = substationlist.SingleOrDefault(x => x.Id == id1);
                SubstationEntity pe2 = substationlist.SingleOrDefault(x => x.Id == id2);

                if (pe1 != null && pe2 != null)
                {
                    Point s = new Point();
                    s.X = pe1.X;
                    s.Y = pe1.Y;
                    Point d = new Point();
                    d.X = pe2.X;
                    d.Y = pe2.Y;

                    foreach (Tuple<Point, Point> tup in linije)
                    {
                        if ((s.X == tup.Item1.X && s.Y == tup.Item1.Y) && (d.X == tup.Item2.X && d.Y == tup.Item2.Y))
                        {
                            flag = true;
                        }
                        else if ((s.X == tup.Item2.X && s.Y == tup.Item2.Y) && (d.X == tup.Item1.X && d.X == tup.Item1.Y))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {

                        if (substationlist.Any(x => x.X == s.X && x.Y == s.Y) && substationlist.Any(x => x.Y == d.Y && x.Y == d.Y))
                        {
                            Point p1 = new Point();
                            Point p2 = new Point();
                            p1.X = s.X;
                            p1.Y = s.Y;
                            p2.X = d.X;
                            p2.Y = d.Y;

                            linije.Add(new Tuple<Point, Point>(p1, p2));

                            leminX = subminX;
                            lemaxX = submaxX;
                            leminY = subminY;
                            lemaxY = submaxY;

                            s.X = Math.Round((((s.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            s.Y = Math.Round((((s.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);

                            d.X = Math.Round((((d.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            d.Y = Math.Round((((d.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            matrix[(int)d.X, (int)d.Y] = 6;

                            Paths(s, le);

                            linentitylist.ToList().Remove(le);

                        }

                    }
                }

            }
        }
        private void IterateOverLines4()
        {
            foreach (LineEntity le in linentitylist)
            {
                bool flag = false;
                long id1 = le.FirstEnd;
                long id2 = le.SecondEnd;
                SwitchEntity pe1 = switchlist.SingleOrDefault(x => x.Id == id1);
                SwitchEntity pe2 = switchlist.SingleOrDefault(x => x.Id == id2);

                if (pe1 != null && pe2 != null)
                {
                    Point s = new Point();
                    s.X = pe1.X;
                    s.Y = pe1.Y;
                    Point d = new Point();
                    d.X = pe2.X;
                    d.Y = pe2.Y;

                    foreach (Tuple<Point, Point> tup in linije)
                    {
                        if ((s.X == tup.Item1.X && s.Y == tup.Item1.Y) && (d.X == tup.Item2.X && d.Y == tup.Item2.Y))
                        {
                            flag = true;
                        }
                        else if ((s.X == tup.Item2.X && s.Y == tup.Item2.Y) && (d.X == tup.Item1.X && d.X == tup.Item1.Y))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {

                        if (switchlist.Any(x => x.X == s.X && x.Y == s.Y) && switchlist.Any(x => x.Y == d.Y && x.Y == d.Y))
                        {
                            Point p1 = new Point();
                            Point p2 = new Point();
                            p1.X = s.X;
                            p1.Y = s.Y;
                            p2.X = d.X;
                            p2.Y = d.Y;

                            linije.Add(new Tuple<Point, Point>(p1, p2));

                            leminX = swminX;
                            lemaxX = swmaxX;
                            leminY = swminY;
                            lemaxY = swmaxY;

                            s.X = Math.Round((((s.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            s.Y = Math.Round((((s.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            d.X = Math.Round((((d.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            d.Y = Math.Round((((d.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            matrix[(int)d.X, (int)d.Y] = 6;

                            Paths(s, le);

                            linentitylist.ToList().Remove(le);

                        }

                    }
                }

            }
        }
        private void IterateOverLines5()
        {
            foreach (LineEntity le in linentitylist)
            {
                bool flag = false;
                long id1 = le.FirstEnd;
                long id2 = le.SecondEnd;
                SwitchEntity pe1 = switchlist.SingleOrDefault(x => x.Id == id1);
                SubstationEntity pe2 = substationlist.SingleOrDefault(x => x.Id == id2);

                if (pe1 != null && pe2 != null)
                {
                    Point s = new Point();
                    s.X = pe1.X;
                    s.Y = pe1.Y;
                    Point d = new Point();
                    d.X = pe2.X;
                    d.Y = pe2.Y;

                    foreach (Tuple<Point, Point> tup in linije)
                    {
                        if ((s.X == tup.Item1.X && s.Y == tup.Item1.Y) && (d.X == tup.Item2.X && d.Y == tup.Item2.Y))
                        {
                            flag = true;
                        }
                        else if ((s.X == tup.Item2.X && s.Y == tup.Item2.Y) && (d.X == tup.Item1.X && d.X == tup.Item1.Y))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {

                        if (switchlist.Any(x => x.X == s.X && x.Y == s.Y) && substationlist.Any(x => x.Y == d.Y && x.Y == d.Y))
                        {
                            Point p1 = new Point();
                            Point p2 = new Point();
                            p1.X = s.X;
                            p1.Y = s.Y;
                            p2.X = d.X;
                            p2.Y = d.Y;

                            linije.Add(new Tuple<Point, Point>(p1, p2));

                            leminX = swminX;
                            lemaxX = swmaxX;
                            leminY = swminY;
                            lemaxY = swmaxY;

                            s.X = Math.Round((((s.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            s.Y = Math.Round((((s.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            leminX = subminX;
                            lemaxX = submaxX;
                            leminY = subminY;
                            lemaxY = submaxY;

                            d.X = Math.Round((((d.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            d.Y = Math.Round((((d.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            matrix[(int)d.X, (int)d.Y] = 6;

                            Paths(s, le);

                            linentitylist.ToList().Remove(le);

                        }

                    }
                }

            }
        }
        private void IterateOverLines6()
        {
            foreach (LineEntity le in linentitylist)
            {
                bool flag = false;
                long id1 = le.FirstEnd;
                long id2 = le.SecondEnd;
                PowerEntity pe1 = switchlist.SingleOrDefault(x => x.Id == id1);
                PowerEntity pe2 = nodesentity.SingleOrDefault(x => x.Id == id2);

                if (pe1 != null && pe2 != null)
                {
                    Point s = new Point();
                    s.X = pe1.X;
                    s.Y = pe1.Y;
                    Point d = new Point();
                    d.X = pe2.X;
                    d.Y = pe2.Y;

                    foreach (Tuple<Point, Point> tup in linije)
                    {
                        if ((s.X == tup.Item1.X && s.Y == tup.Item1.Y) && (d.X == tup.Item2.X && d.Y == tup.Item2.Y))
                        {
                            flag = true;
                        }
                        else if ((s.X == tup.Item2.X && s.Y == tup.Item2.Y) && (d.X == tup.Item1.X && d.X == tup.Item1.Y))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {

                        if (switchlist.Any(x => x.X == s.X && x.Y == s.Y) && nodesentity.Any(x => x.Y == d.Y && x.Y == d.Y))
                        {
                            Point p1 = new Point();
                            Point p2 = new Point();
                            p1.X = s.X;
                            p1.Y = s.Y;
                            p2.X = d.X;
                            p2.Y = d.Y;

                            linije.Add(new Tuple<Point, Point>(p1, p2));

                            leminX = swminX;
                            lemaxX = swmaxX;
                            leminY = swminY;
                            lemaxY = swmaxY;

                            s.X = Math.Round((((s.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            s.Y = Math.Round((((s.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            leminX = nodeminX;
                            lemaxX = nodemaxX;
                            leminY = nodeminY;
                            lemaxY = nodemaxY;
                            d.X = Math.Round((((d.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            d.Y = Math.Round((((d.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            matrix[(int)d.X, (int)d.Y] = 6;

                            Paths(s, le);

                            linentitylist.ToList().Remove(le);

                        }

                    }
                }

            }
        }
        private void IterateOverLines7()
        {
            foreach (LineEntity le in linentitylist)
            {
                bool flag = false;
                long id1 = le.FirstEnd;
                long id2 = le.SecondEnd;
                PowerEntity pe1 = nodesentity.SingleOrDefault(x => x.Id == id1);
                PowerEntity pe2 = nodesentity.SingleOrDefault(x => x.Id == id2);

                if (pe1 != null && pe2 != null)
                {
                    Point s = new Point();
                    s.X = pe1.X;
                    s.Y = pe1.Y;
                    Point d = new Point();
                    d.X = pe2.X;
                    d.Y = pe2.Y;

                    foreach (Tuple<Point, Point> tup in linije)
                    {
                        if ((s.X == tup.Item1.X && s.Y == tup.Item1.Y) && (d.X == tup.Item2.X && d.Y == tup.Item2.Y))
                        {
                            flag = true;
                        }
                        else if ((s.X == tup.Item2.X && s.Y == tup.Item2.Y) && (d.X == tup.Item1.X && d.X == tup.Item1.Y))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {

                        if (nodesentity.Any(x => x.X == s.X && x.Y == s.Y) && nodesentity.Any(x => x.Y == d.Y && x.Y == d.Y))
                        {
                            Point p1 = new Point();
                            Point p2 = new Point();
                            p1.X = s.X;
                            p1.Y = s.Y;
                            p2.X = d.X;
                            p2.Y = d.Y;

                            linije.Add(new Tuple<Point, Point>(p1, p2));

                            leminX = nodeminX;
                            lemaxX = nodemaxX;
                            leminY = nodeminY;
                            lemaxY = nodemaxY;

                            s.X = Math.Round((((s.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            s.Y = Math.Round((((s.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);

                            d.X = Math.Round((((d.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            d.Y = Math.Round((((d.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            matrix[(int)d.X, (int)d.Y] = 6;

                            Paths(s, le);

                            linentitylist.ToList().Remove(le);

                        }

                    }
                }

            }
        }
        private void IterateOverLines8()
        {
            foreach (LineEntity le in linentitylist)
            {
                bool flag = false;
                long id1 = le.FirstEnd;
                long id2 = le.SecondEnd;
                PowerEntity pe1 = nodesentity.SingleOrDefault(x => x.Id == id1);
                PowerEntity pe2 = switchlist.SingleOrDefault(x => x.Id == id2);

                if (pe1 != null && pe2 != null)
                {
                    Point s = new Point();
                    s.X = pe1.X;
                    s.Y = pe1.Y;
                    Point d = new Point();
                    d.X = pe2.X;
                    d.Y = pe2.Y;

                    foreach (Tuple<Point, Point> tup in linije)
                    {
                        if ((s.X == tup.Item1.X && s.Y == tup.Item1.Y) && (d.X == tup.Item2.X && d.Y == tup.Item2.Y))
                        {
                            flag = true;
                        }
                        else if ((s.X == tup.Item2.X && s.Y == tup.Item2.Y) && (d.X == tup.Item1.X && d.X == tup.Item1.Y))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {

                        if (nodesentity.Any(x => x.X == s.X && x.Y == s.Y) && switchlist.Any(x => x.Y == d.Y && x.Y == d.Y))
                        {
                            Point p1 = new Point();
                            Point p2 = new Point();
                            p1.X = s.X;
                            p1.Y = s.Y;
                            p2.X = d.X;
                            p2.Y = d.Y;

                            linije.Add(new Tuple<Point, Point>(p1, p2));

                            leminX = nodeminX;
                            lemaxX = nodemaxX;
                            leminY = nodeminY;
                            lemaxY = nodemaxY;

                            s.X = Math.Round((((s.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            s.Y = Math.Round((((s.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);

                            leminX = swminX;
                            lemaxX = swmaxX;
                            leminY = swminY;
                            lemaxY = swmaxY;

                            d.X = Math.Round((((d.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            d.Y = Math.Round((((d.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            matrix[(int)d.X, (int)d.Y] = 6;

                            Paths(s, le);

                            linentitylist.ToList().Remove(le);

                        }

                    }
                }

            }
        }
        private void IterateOverLines9()
        {
            foreach (LineEntity le in linentitylist)
            {
                bool flag = false;
                long id1 = le.FirstEnd;
                long id2 = le.SecondEnd;
                PowerEntity pe1 = nodesentity.SingleOrDefault(x => x.Id == id1);
                PowerEntity pe2 = substationlist.SingleOrDefault(x => x.Id == id2);

                if (pe1 != null && pe2 != null)
                {
                    Point s = new Point();
                    s.X = pe1.X;
                    s.Y = pe1.Y;
                    Point d = new Point();
                    d.X = pe2.X;
                    d.Y = pe2.Y;

                    foreach (Tuple<Point, Point> tup in linije)
                    {
                        if ((s.X == tup.Item1.X && s.Y == tup.Item1.Y) && (d.X == tup.Item2.X && d.Y == tup.Item2.Y))
                        {
                            flag = true;
                        }
                        else if ((s.X == tup.Item2.X && s.Y == tup.Item2.Y) && (d.X == tup.Item1.X && d.X == tup.Item1.Y))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {

                        if (nodesentity.Any(x => x.X == s.X && x.Y == s.Y) && substationlist.Any(x => x.Y == d.Y && x.Y == d.Y))
                        {
                            Point p1 = new Point();
                            Point p2 = new Point();
                            p1.X = s.X;
                            p1.Y = s.Y;
                            p2.X = d.X;
                            p2.Y = d.Y;

                            linije.Add(new Tuple<Point, Point>(p1, p2));

                            leminX = nodeminX;
                            lemaxX = nodemaxX;
                            leminY = nodeminY;
                            lemaxY = nodemaxY;

                            s.X = Math.Round((((s.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            s.Y = Math.Round((((s.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            leminX = subminX;
                            lemaxX = submaxX;
                            leminY = subminY;
                            lemaxY = submaxY;

                            d.X = Math.Round((((d.X - leminX) * (99 - 0)) / (lemaxX - leminX)) + 0);
                            d.Y = Math.Round((((d.Y - leminY) * (99 - 0)) / (lemaxY - leminY)) + 0);


                            matrix[(int)d.X, (int)d.Y] = 6;

                            Paths(s, le);

                            linentitylist.ToList().Remove(le);

                        }

                    }
                }

            }
        }




        private void Paths(Point s, LineEntity le)
        {
            bool dvojka = false;
            bool nula = false;

            Node destination = BFS_Algorithm(matrix, (int)s.X, (int)s.Y, false);


            if (destination != null)
            {

                List<System.Windows.Point> points = new List<System.Windows.Point>();

                if (matrix[destination.Previous.I, destination.Previous.J] == 2)
                {
                    dvojka = true;
                }
                else if (matrix[destination.Previous.I, destination.Previous.J] == 0)
                {
                    nula = true;
                }


                for (int i = destination.Vr; i >= 0; i--)
                {

                    if (dvojka == true && nula == false)
                    {
                        points.Add(new System.Windows.Point((((destination.I - 0) * (MyCanvas.ActualWidth - 0)) / (99 - 0)), (((destination.J - 0) * (MyCanvas.ActualHeight - 0)) / (99 - 0))));

                        if (destination.Previous != null && matrix[destination.Previous.I, destination.Previous.J] == 0)
                        {

                            Path p = new Path();
                            EllipseGeometry tacka = new EllipseGeometry();
                            tacka.RadiusX = 1;
                            tacka.RadiusY = 1;

                            tacka.Center = new System.Windows.Point((((destination.I - 0) * (MyCanvas.ActualWidth - 0)) / (99 - 0)), (((destination.J - 0) * (MyCanvas.ActualHeight - 0)) / (99 - 0)));
                            p.Name = "enabled";
                            p.Fill = System.Windows.Media.Brushes.Aqua;
                            p.Data = tacka;
                            MyCanvas.Children.Add(p);
                            dvojka = false;
                            nula = true;
                        }

                        destination = destination.Previous;
                    }
                    else if (nula == true && dvojka == false)
                    {
                        points.Add(new System.Windows.Point((((destination.I - 0) * (MyCanvas.ActualWidth - 0)) / (99 - 0)), (((destination.J - 0) * (MyCanvas.ActualHeight - 0)) / (99 - 0))));
                        matrix[destination.I, destination.J] = 2;


                        if (destination.Previous != null && matrix[destination.Previous.I, destination.Previous.J] == 2)
                        {

                            Path p = new Path();
                            EllipseGeometry tacka = new EllipseGeometry();
                            tacka.RadiusX = 1;
                            tacka.RadiusY = 1;

                            tacka.Center = new System.Windows.Point((((destination.Previous.I - 0) * (MyCanvas.ActualWidth - 0)) / (99 - 0)), (((destination.Previous.J - 0) * (MyCanvas.ActualHeight - 0)) / (99 - 0)));
                            p.Name = "enabled";
                            p.Fill = System.Windows.Media.Brushes.Aqua;
                            p.Data = tacka;
                            MyCanvas.Children.Add(p);
                            dvojka = true;
                            nula = false;
                        }

                        destination = destination.Previous;
                    }
                    else
                    {
                        points.Add(new System.Windows.Point((((destination.I - 0) * (MyCanvas.ActualWidth - 0)) / (99 - 0)), (((destination.J - 0) * (MyCanvas.ActualHeight - 0)) / (99 - 0))));
                        destination = destination.Previous;
                    }
                }
                Polyline line = new Polyline();
                PointCollection collection = new PointCollection();
                foreach (System.Windows.Point p in points)
                {
                    collection.Add(p);
                }
                line.Points = collection;
                line.Stroke = new SolidColorBrush(Colors.Aquamarine);
                line.StrokeThickness = 0.5;
                line.SetValue(ToolTipService.ToolTipProperty, le.GetType().ToString().Substring(14) + "\nID: " + le.Id +
                    "\nLine type: " + le.LineType + "\nName: " + le.Name + "\nIs Underground: " + le.IsUnderground
                    + "\nThermalConstantHeat: " + le.ThermalConstantHeat + "\nR: " + le.R + "\nConductor Material: " + le.ConductorMaterial);
                MyCanvas.Children.Add(line);
                polylinije.Add(line);
            }
        }

        public static System.Windows.Point[] GetIntersectionPoints(Geometry g1, Geometry g2)
        {
            Geometry og1 = g1.GetWidenedPathGeometry(new System.Windows.Media.Pen(System.Windows.Media.Brushes.Black, 1.0f));
            Geometry og2 = g2.GetWidenedPathGeometry(new System.Windows.Media.Pen(System.Windows.Media.Brushes.Black, 1.0f));
            CombinedGeometry cg = new CombinedGeometry(GeometryCombineMode.Intersect, og1, og2);
            PathGeometry pg = cg.GetFlattenedPathGeometry();
            System.Windows.Point[] result = new System.Windows.Point[pg.Figures.Count];
            for (int i = 0; i < pg.Figures.Count; i++)
            {
                Rect fig = new PathGeometry(new PathFigure[] { pg.Figures[i] }).Bounds;
                result[i] = new System.Windows.Point(fig.Left + fig.Width / 2.0, fig.Top + fig.Height / 2.0);
            }
            return result;
        }

        private void AddToCanvas(double minX, double minY, double maxX, double maxY, double radiusX, double radiusY, System.Windows.Media.Brush brush, PowerEntity node)
        {
            Point p1 = new Point();
            p1.X = node.X;
            p1.Y = node.Y;

            if (p1.X % 1 != 0 && p1.Y % 1 != 0)
            {
                p1.X = Math.Round((((p1.X - minX) * (99 - 0)) / (maxX - minX)) + 0);
                p1.Y = Math.Round((((p1.Y - minY) * (99 - 0)) / (maxY - minY)) + 0);
            }

            if (matrix[(int)p1.X, (int)p1.Y] == 1)
            {
                Node nodetemp = BFS_Algorithm2(matrix, (int)p1.X, (int)p1.Y);
                p1.X = nodetemp.I;
                p1.Y = nodetemp.J;

                if (node.GetType().Equals(typeof(SubstationEntity)))
                {
                    node.X = ((((p1.X - 0) * (submaxX - subminX)) / (99 - 0)) + subminX);
                    node.Y = ((((p1.Y - 0) * (submaxY - subminY)) / (99 - 0)) + subminY);
                }
                else if (node.GetType().Equals(typeof(SwitchEntity)))
                {
                    node.X = ((((p1.X - 0) * (swmaxX - swminX)) / (99 - 0)) + swminX);
                    node.Y = ((((p1.Y - 0) * (swmaxY - swminY)) / (99 - 0)) + swminY);
                }
                else if (node.GetType().Equals(typeof(SwitchEntity)))
                {
                    node.X = ((((p1.X - 0) * (nodemaxX - nodeminX)) / (99 - 0)) + nodeminX);
                    node.Y = ((((p1.Y - 0) * (nodemaxY - nodeminY)) / (99 - 0)) + nodeminY);
                }

            }

            matrix[(int)p1.X, (int)p1.Y] = 1;

            double temppointx = (((p1.X - 0) * (MyCanvas.ActualWidth - 0)) / (99 - 0));
            double temppointy = (((p1.Y - 0) * (MyCanvas.ActualHeight - 0)) / (99 - 0));

            Path p = new Path();
            EllipseGeometry tacka = new EllipseGeometry();
            tacka.RadiusX = radiusX;
            tacka.RadiusY = radiusY;

            tacka.Center = new System.Windows.Point(temppointx, temppointy);
            p.Name = "enabled";
            p.Fill = brush;
            p.Data = tacka;
            if (node.GetType().Equals(typeof(SwitchEntity)))
            {
                SwitchEntity sw = (SwitchEntity)node;
                p.SetValue(ToolTipService.ToolTipProperty, node.GetType().ToString().Substring(14) + "\nID: " + node.Id + "\nName: " + node.Name + "\nStatus: " + sw.Status);
            }
            else
            {
                p.SetValue(ToolTipService.ToolTipProperty, node.GetType().ToString().Substring(14) + "\nID: " + node.Id + "\nName: " + node.Name);

            }

            p.MouseLeftButtonDown += NodeClickedOn;
            MyCanvas.Children.Add(p);
        }

        private void NodeClickedOn(object sender, MouseButtonEventArgs e)
        {
            Path path = (Path)sender;
            EllipseGeometry ellipse = (EllipseGeometry)path.Data;

            DoubleAnimation doubleAnimationX = new DoubleAnimation();
            DoubleAnimation doubleAnimationY = new DoubleAnimation();

            doubleAnimationX.From = ellipse.RadiusX;
            doubleAnimationX.To = 10;
            doubleAnimationX.Duration = new Duration(TimeSpan.FromSeconds(1));
            doubleAnimationX.AutoReverse = true;

            doubleAnimationY.From = ellipse.RadiusY;
            doubleAnimationY.To = 10;
            doubleAnimationY.Duration = new Duration(TimeSpan.FromSeconds(1));
            doubleAnimationY.AutoReverse = true;

            ellipse.BeginAnimation(EllipseGeometry.RadiusXProperty, doubleAnimationX);
            ellipse.BeginAnimation(EllipseGeometry.RadiusYProperty, doubleAnimationY);

        }


        private Node BFS_Algorithm(int[,] grid, int i, int j, bool flag)
        {

            Node[,] dist = new Node[100, 100];

            for (int i1 = 0; i1 < 100; i1++)
            {
                for (int j1 = 0; j1 < 100; j1++)
                {
                    dist[i1, j1] = new Node(i1, j1);
                }
            }

            Queue<Tuple<int, int>> q = new Queue<Tuple<int, int>>();
            q.Enqueue(new Tuple<int, int>(i, j));
            grid[i, j] = 1;
            while (q.Count != 0)
            {
                Tuple<int, int> item = q.Dequeue();
                i = item.Item1;
                j = item.Item2;
                if (grid[i, j] == 6)
                {
                    grid[i, j] = 1;
                    return dist[i, j];
                }
                for (int k = 0; k < 4; k++)
                {
                    int u = i + dx[k];
                    int v = j + dy[k];
                    if (isvalid(grid, u, v))
                    {
                        if (dist[u, v].Vr == 0)
                        {
                            if (flag)
                                return dist[u, v];
                            dist[u, v].Vr = dist[i, j].Vr + 1;
                            dist[u, v].Previous = dist[i, j];
                            q.Enqueue(new Tuple<int, int>(u, v));
                        }

                    }
                }
            }

            return null;
        }

        private Node BFS_Algorithm2(int[,] grid, int i, int j)
        {

            Node[,] dist = new Node[100, 100];

            for (int i1 = 0; i1 < 100; i1++)
            {
                for (int j1 = 0; j1 < 100; j1++)
                {
                    dist[i1, j1] = new Node(i1, j1);
                }
            }

            Queue<Tuple<int, int>> q = new Queue<Tuple<int, int>>();
            q.Enqueue(new Tuple<int, int>(i, j));
            grid[i, j] = 1;
            while (q.Count != 0)
            {
                Tuple<int, int> item = q.Dequeue();
                i = item.Item1;
                j = item.Item2;
                if (grid[i, j] == 0)
                {
                    grid[i, j] = 1;
                    return dist[i, j];
                }
                for (int k = 0; k < 4; k++)
                {
                    int u = i + dx[k];
                    int v = j + dy[k];
                    if (isvalid2(grid, u, v))
                    {

                        if (dist[u, v].Vr == 0)
                        {
                            dist[u, v].Vr = dist[i, j].Vr + 1;
                            dist[u, v].Previous = dist[i, j];
                            q.Enqueue(new Tuple<int, int>(u, v));
                        }

                    }
                }
            }

            return null;
        }

        private bool isvalid(int[,] grid, int x, int y)
        {
            return x >= 0 && y >= 0 && x < n && y < m && (grid[x, y] != 1 /*&& grid[x, y] != 2 */);
        }


        private bool isvalid2(int[,] grid, int x, int y)
        {
            return x >= 0 && y >= 0 && x < n && y < m;
        }


        private void MyCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

            for (int i = 0; i < uielements.Count; i++)
            {
                if (uielements[i].GetType().Equals(typeof(Path)))
                {
                    Path path = (Path)uielements[i];
                    path.Fill = brushes[i];
                }
                else if (uielements[i].GetType().Equals(typeof(Polyline)))
                {
                    Polyline path = (Polyline)uielements[i];
                    path.Stroke = brushes[i];
                }
            }

            brushes.Clear();
            uielements.Clear();

            System.Windows.Point pt = e.GetPosition((UIElement)sender);

            // Clear the contents of the list used for hit test results.
            hitResults.Clear();

            // Set up a callback to receive the hit test result enumeration.
            VisualTreeHelper.HitTest(MyCanvas, null,
                new HitTestResultCallback(MyHitTestResult),
                new PointHitTestParameters(pt));

            var brush = System.Windows.Media.Brushes.Green;
            {
                foreach (UIElement ui in hitResults)
                {
                    if (ui.GetType().Equals(typeof(System.Windows.Shapes.Polyline)))
                    {
                        Polyline p = (Polyline)ui;
                        brushes.Add(p.Stroke);
                        p.Stroke = brush;
                        uielements.Add(ui);
                        var p1 = p.Points.First();
                        var p2 = p.Points.Last();
                        foreach (UIElement tacka in MyCanvas.Children)
                        {
                            if (tacka.GetType().Equals(typeof(Path)))
                            {
                                var temptacka = (Path)tacka;
                                var tempelipsa = (EllipseGeometry)temptacka.Data;
                                if ((tempelipsa.Center.X == p1.X && tempelipsa.Center.Y == p1.Y) || (tempelipsa.Center.Y == p2.Y && tempelipsa.Center.X == p2.X))
                                {
                                    brushes.Add(temptacka.Fill);
                                    temptacka.Fill = brush;
                                    uielements.Add(tacka);
                                }
                            }
                        }
                    }


                }
            }
        }

        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            hitResults.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        //From UTM to Latitude and longitude in decimal
        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }
    }

}
