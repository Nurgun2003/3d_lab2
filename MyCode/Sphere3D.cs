using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace lab2.MyCode
{
    public class Sphere3D : ModelVisual3D
    {
        private const int max_i = 60; //на сколько полос по долготе (longitudes)
        private const int max_j = 60; //на сколько полос по широте (latitudes)
        private Point3D[,] position = new Point3D[max_i + 1, max_j]; //массив для точек
        private Point[,] texture = new Point[max_i + 1, max_j];

        private void GenerateSphere(int longitudes, int latitudes)
        {
            double latitudeArcusIncrement = Math.PI / (latitudes - 1);
            double longitudeArcusIncrement = 2.0 * Math.PI / longitudes;
            for (int lat = 0; lat < latitudes; lat++)
            {
                double latitudeArcus = lat * latitudeArcusIncrement; //угол по вертикали
                double radius = Math.Sin(latitudeArcus); //радиус цилиндра
                double y = Math.Cos(latitudeArcus);
                double textureY = (double)lat / (latitudes - 1);
                for (int lon = 0; lon <= longitudes; lon++)
                {
                    double longitudeArcus = lon * longitudeArcusIncrement;
                    position[lon, lat].X = radius * Math.Cos(longitudeArcus);
                    position[lon, lat].Y = y;
                    position[lon, lat].Z = -radius * Math.Sin(longitudeArcus);
                    texture[lon, lat].X = (double)lon / longitudes;
                    texture[lon, lat].Y = textureY;
                }
            }
        }

        public BitmapImage moonImage = new BitmapImage(new Uri("moon.bmp", UriKind.Relative));
        //массив для материалов, т.е. кисти для текстуры
        private DiffuseMaterial[] frontMaterial = new DiffuseMaterial[max_j - 1];

        private void GenerateImageMaterials()
        {
            ImageBrush imageBrush;
            double flatThickness = 1.0 / (max_i - 1); // ширина полосы
            double minus = (double)(max_i);//???
            for (int i = 0; i < max_i - 1; i++)
            {
                imageBrush = new ImageBrush((BitmapImage)moonImage);// для каждой полосы готовим кисть
                                                                     //задаем активную область текстуры для кисти. 
                                                                     //Это относится ко всей полосе цилиндра
                imageBrush.Viewbox = new Rect(0, i * flatThickness, minus / max_i, flatThickness);
                frontMaterial[i] = new DiffuseMaterial(imageBrush);
            }
        }

        private void GenerateAllCylinders()
        {
            //Создаем объект Model3DGroup для всей сферы
            Model3DGroup model3DGroup = new Model3DGroup();
            for (int lat = 0; lat < max_j - 1; lat++)//цикл по всем полосам
            {
                //Создаем объект GeometryModel3D для полосы
                GeometryModel3D geometryModel3D = new GeometryModel3D();
                geometryModel3D.Geometry = GenerateCylinder(lat); //заполнение треугольников полосы
                geometryModel3D.Material = frontMaterial[lat];//назначение текстурной кисти
                model3DGroup.Children.Add(geometryModel3D);
            }
            //Созданный объект model3DGroup является геометрическим представлением сферы
            Content = model3DGroup;
        }

        private MeshGeometry3D GenerateCylinder(int lat)
        {
            MeshGeometry3D meshGeometry3D = new MeshGeometry3D();
            for (int lon = 0; lon <= max_i; lon++)       //для каждой точки параллели
            {
                Point3D p0 = position[lon, lat];           //точка на параллели
                Point3D p1 = position[lon, lat + 1];       //точка на параллели выше
                meshGeometry3D.Positions.Add(p0);
                meshGeometry3D.Positions.Add(p1);
                meshGeometry3D.Normals.Add((Vector3D)p0);  //Задаем нормали
                meshGeometry3D.Normals.Add((Vector3D)p1);  //
                meshGeometry3D.TextureCoordinates.Add(texture[lon, lat]); // координаты текстуры
                meshGeometry3D.TextureCoordinates.Add(texture[lon, lat + 1]); // координаты текстуры
            }
            for (int lon = 1; lon < meshGeometry3D.Positions.Count - 2; lon += 2)
            { //первый треугольник = левая верхняя часть четырехугольника
                meshGeometry3D.TriangleIndices.Add(lon - 1); //точка левая верхняя
                meshGeometry3D.TriangleIndices.Add(lon); //точка левая нижняя
                meshGeometry3D.TriangleIndices.Add(lon + 1); //точка правая верняя
                                                             //второй треугольник – нижняя правая часть четурехугольника
                meshGeometry3D.TriangleIndices.Add(lon + 1); //точка справа выше
                meshGeometry3D.TriangleIndices.Add(lon); //левая нижняя
                meshGeometry3D.TriangleIndices.Add(lon + 2); //правая нижняя точка
            }
            return meshGeometry3D; //возвращаем полосу
        }

        public Sphere3D()
        {
            GenerateImageMaterials();
            GenerateSphere(max_i, max_j);
            GenerateAllCylinders();
        }
    }
}
