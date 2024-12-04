﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Room
{
    public partial class Form1 : Form
    {
        RayTracing rayTracing;

        Face leftWall;
        Face rightWall;
        Face backWall;
        Face cameraWall;
        Face ceiling;
        Face floor;
        Cube cube;

        public Form1()
        {
            InitializeComponent();

            Vertex center = new Vertex(0, 0, 14);
            double roomSide = 30;

            leftWall = new Face(new Vertex(center.x - roomSide / 2, center.y, center.z),
                new Vector(1, 0, 0),
                new Vector(0, 1, 0),
                roomSide,
                roomSide,
                Color.FromArgb(255, 89, 89),
                new Material(0, 0, 0.9, 0.1, 0, 0));
            rightWall = new Face(new Vertex(center.x + roomSide / 2, center.y, center.z),
                new Vector(-1, 0, 0),
                new Vector(0, 1, 0),
                roomSide,
                roomSide,
                Color.FromArgb(87, 210, 255),
                new Material(0, 0, 0.9, 0.1, 0, 0));
            backWall = new Face(new Vertex(center.x, center.y, center.z + roomSide / 2),
                new Vector(0, 0, -1),
                new Vector(0, 1, 0),
                roomSide,
                roomSide,
                Color.LightGray,
                new Material(0, 0, 0.9, 0.1, 0, 0));
            cameraWall = new Face(new Vertex(center.x, center.y, center.z - roomSide / 2),
                new Vector(0, 0, 1),
                new Vector(0, 1, 0),
                roomSide,
                roomSide,
                Color.Green,
                new Material(0, 0, 0.9, 0.1, 0, 0));
            ceiling = new Face(new Vertex(center.x, center.y + roomSide / 2, center.z),
                new Vector(0, -1, 0),
                new Vector(0, 0, 1),
                roomSide,
                roomSide,
                Color.LightGray,
                new Material(0, 0, 0.9, 0.1, 0, 0));
            floor = new Face(new Vertex(center.x, center.y - roomSide / 2, center.z),
                new Vector(0, 1, 0),
                new Vector(0, 0, 1),
                roomSide,
                roomSide,
                Color.LightGray,
                new Material(0, 0, 0.9, 0.1, 0, 0));

            cube = new Cube(new Vertex(6, -9, 21), 7, Color.White, new Material(40, 0.25, 0.7, 0.05, 0, 0));

            rayTracing = new RayTracing(new Room(center, roomSide, leftWall, rightWall, backWall, floor, ceiling, cameraWall),
                new LightSource(new Vertex(0, 13, 14), 1, Color.FromArgb(255, 255, 240)));

            //rayTracing.addShape(sphereOnCube);

            
            rayTracing.addShape(new Sphere(new Vertex(-5, -8, 20), 5, Color.Bisque, new Material(40, 0.25, 0.7, 0.05, 0, 1)));

            rayTracing.addShape(cameraWall);
            rayTracing.addShape(backWall);
            rayTracing.addShape(ceiling);
            rayTracing.addShape(floor);
            rayTracing.addShape(rightWall);
            rayTracing.addShape(leftWall);
            rayTracing.addShape(cube);

            RenderRoom();
        }

        async void RenderRoom()
        {
            label1.Text = "Рендер...";
            var bitmap = await Task.Run(() => rayTracing.compute(roomPictureBox.Width, roomPictureBox.Height));
            roomPictureBox.Image = bitmap;
            label1.Text = "Готово!";
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            switch (e.Index)
            {
                case 0:
                    leftWall.material.reflectivity = (1 + leftWall.material.reflectivity) % 2;
                    break;
                case 1:
                    rightWall.material.reflectivity = (1 + rightWall.material.reflectivity) % 2;
                    break;
                case 2:
                    cameraWall.material.reflectivity = (1 + cameraWall.material.reflectivity) % 2;
                    break;
                case 3:
                    backWall.material.reflectivity = (1 + backWall.material.reflectivity) % 2;
                    break;
                case 4:
                    floor.material.reflectivity = (1 + floor.material.reflectivity) % 2;
                    break;
                case 5:
                    ceiling.material.reflectivity = (1 + ceiling.material.reflectivity) % 2;
                    break;
            }

            RenderRoom();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb.Checked)
            {
                rayTracing.AddLightSource((int)numericUpDown1.Value,
                    (int)numericUpDown2.Value,
                    (int)numericUpDown3.Value);
                numericUpDown1.Enabled = true;
                numericUpDown2.Enabled = true;
                numericUpDown3.Enabled = true;
            }
            else
            {
                rayTracing.RemoveLightSource();
                numericUpDown1.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown3.Enabled = false;
            }
            RenderRoom();
        }

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            rayTracing.ChangeAddLightPos((int)numericUpDown1.Value,
                    (int)numericUpDown2.Value,
                    (int)numericUpDown3.Value);
            RenderRoom();
        }
    }

    class RayTracing
    {
        List<Shape> figures;
        const double fov = 80;
        Vertex cameraPosition = new Vertex(0, 0, 0);
        List<LightSource> lightSources = new List<LightSource>();
        Room room;

        public RayTracing(Room room, LightSource ls)
        {
            this.room = room;
            figures = new List<Shape>();
            lightSources.Add(ls);
        }

        public void addShape(Shape shape)
        {
            figures.Add(shape);
        }

        public void AddLightSource(int x, int y, int z)
        {
            lightSources.Add(new LightSource(new Vertex(x, y, z), 0.5, Color.FromArgb(181, 255, 201)));
            lightSources[0].intensity = 0.5;
        }

        public void ChangeAddLightPos(int x, int y, int z)
        {
            lightSources[1].location = new Vertex(x, y, z);
        }

        public void RemoveLightSource()
        {
            lightSources.RemoveAt(1);
            lightSources[0].intensity = 1;
        }

        static Color changeColorIntensity(Color color, double intensity, List<LightSource> lightSources)
        {
            double finalR = 0;
            double finalG = 0;
            double finalB = 0;

            foreach (var ls in lightSources)
            {
                if (ls.intensity < 0)
                    continue;

                double lightR = ls.color.R / 255.0;
                double lightG = ls.color.G / 255.0;
                double lightB = ls.color.B / 255.0;

                finalR += color.R * lightR * ls.intensity;
                finalG += color.G * lightG * ls.intensity;
                finalB += color.B * lightB * ls.intensity;
            }


            byte newR = (byte)MyMath.Clamp(Math.Round(finalR * intensity), 0, 255);
            byte newG = (byte)MyMath.Clamp(Math.Round(finalG * intensity), 0, 255);
            byte newB = (byte)MyMath.Clamp(Math.Round(finalB * intensity), 0, 255);

            return Color.FromArgb(newR, newG, newB);
        }

        bool doesRayIntersectSomething(Vector direction, Vertex origin)
        {
            foreach (var shape in figures)
            {
                if (shape is Face)
                {
                    continue;
                }
                if (shape.getIntersection(direction, origin) != null)
                {
                    return true;
                }
            }

            return false;
        }

        Vector getLightReflectionRay(Vector shadowRay, Vector normale)
        {
            return (2 * (shadowRay ^ normale) * normale - shadowRay).normalize();
        }

        Vector getViewReflectionRay(Vector viewRay, Vector normale)
        {
            return (2 * ((-1 * viewRay) ^ normale) * normale - (-1 * viewRay)).normalize();
        }

        double computeLightness(Shape shape, Tuple<Vertex, Vector> intersectionAndNormale, Vector viewRay)
        {
            double diffuseLightness = 0;
            double specularLightness = 0;
            double ambientLightness = 1;

            foreach (LightSource ls in lightSources)
            {
                var shadowRay = new Vector(intersectionAndNormale.Item1, ls.location, true);
                var reflectionRay = getLightReflectionRay(shadowRay, intersectionAndNormale.Item2);

                if (doesRayIntersectSomething(shadowRay, intersectionAndNormale.Item1))
                    return ambientLightness * shape.material.kambient +
                                    diffuseLightness * shape.material.kdiffuse +
                                    specularLightness * shape.material.kspecular;

                diffuseLightness += ls.intensity * MyMath.Clamp(shadowRay ^ intersectionAndNormale.Item2,
                    0.0, double.MaxValue);
                specularLightness += ls.intensity *
                                     Math.Pow(MyMath.Clamp(reflectionRay ^ (-1 * viewRay), 0.0, double.MaxValue),
                                         shape.material.shininess);
            }
            return ambientLightness * shape.material.kambient +
                                    diffuseLightness * shape.material.kdiffuse +
                                    specularLightness * shape.material.kspecular;
        }

        Color mixColors(Color first, Color second, double secondToFirstRatio)
        {
            return Color.FromArgb((byte)((second.R * secondToFirstRatio) + first.R * (1 - secondToFirstRatio)), (byte)((second.G * secondToFirstRatio) + first.G * (1 - secondToFirstRatio)), (byte)((second.B * secondToFirstRatio) + first.B * (1 - secondToFirstRatio)));
        }

        Color shootRay(Vector viewRay, Vertex origin, int depth = 0)
        {
            double nearestVertex = double.MaxValue;
            if (depth > 4)
            {
                return Color.Gray;
            }
            Color res = Color.Black;
            foreach (var shape in figures)
            {
                Tuple<Vertex, Vector> intersectionAndNormale;
                if ((intersectionAndNormale = shape.getIntersection(viewRay, origin)) != null &&
                    intersectionAndNormale.Item1.z < nearestVertex)
                {
                    nearestVertex = intersectionAndNormale.Item1.z;
                    res = changeColorIntensity(shape.color, computeLightness(shape, intersectionAndNormale, viewRay), lightSources);
                    if (shape.material.reflectivity > 0)
                    {
                        var reflectedColor = shootRay(getViewReflectionRay(viewRay, intersectionAndNormale.Item2), intersectionAndNormale.Item1, depth + 1);
                        res = mixColors(res, reflectedColor, shape.material.reflectivity);
                    }
                    if (shape.material.transparency > 0)
                    {

                    }
                }
            }
            return res;
        }

        public Bitmap compute(int width, int height)
        {
            var bitmap = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector ray = new Vector(
                        (2 * (x + 0.5) / width - 1) * Math.Tan(MyMath.DegsToRads(fov / 2)) *
                        width / height,
                        -(2 * (y + 0.5) / height - 1) * Math.Tan(MyMath.DegsToRads(fov / 2)),
                        1, true);
                    var color = shootRay(ray, cameraPosition);
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }
    }

    class Sphere : Shape
    {
        double radius;

        public Sphere(Vertex center, double radius, Color color, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.color = color;
            this.material = material;
        }


        public override Tuple<Vertex, Vector> getIntersection(Vector direction, Vertex origin)
        {
            direction = direction.normalize();
            Vector sourceToCenter = new Vector(origin, center);
            if ((sourceToCenter ^ direction) < 0)  // Центр сферы за точкой выпуска луча
            {
                if (MyMath.Distance(origin, center) > radius)                       // Пересечения нет
                    return null;
                else if (MyMath.Distance(origin, center) - radius < 0.000001)       // Мы на сфере
                    return null;
                else                                                                // Мы внутри сферы
                {
                    Vertex projection = MyMath.GetVertexProjection(origin, direction, center);
                    double Distance = Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(MyMath.Distance(center, projection), 2)) - MyMath.Distance(origin, projection);
                    var intersection = MyMath.VertexOnLine(origin, direction, Distance);
                    return Tuple.Create(intersection, new Vector(center, intersection, true));
                }
            }
            else        // Центр сферы можно спроецировать на луч
            {
                Vertex projection = MyMath.GetVertexProjection(origin, direction, center);
                if (MyMath.Distance(center, projection) > radius)
                {
                    return null;
                }
                else
                {
                    double Distance = Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(MyMath.Distance(center, projection), 2));
                    if (MyMath.Distance(origin, center) > radius)
                    {
                        Distance = MyMath.Distance(origin, projection) - Distance;
                    }
                    else
                    {
                        Distance = MyMath.Distance(origin, projection) + Distance;
                    }
                    var intersection = MyMath.VertexOnLine(origin, direction, Distance);
                    return Tuple.Create(intersection, new Vector(center, intersection, true));
                }
            }
        }
    }

    class Cube : Shape
    {
        double side;
        private List<Face> faces;
        public Cube(Vertex center, double side, Color color, Material material)
        {
            this.center = center;
            this.side = side;
            this.color = color;
            this.material = material;
            faces = new List<Face>();
            faces.Add(new Face(new Vertex(center.x, center.y, center.z - side / 2), new Vector(0, 0, -1), new Vector(0, 1, 0), side, side));
            faces.Add(new Face(new Vertex(center.x, center.y, center.z + side / 2), new Vector(0, 0, 1), new Vector(0, 1, 0), side, side));
            faces.Add(new Face(new Vertex(center.x, center.y + side / 2, center.z), new Vector(0, 1, 0), new Vector(0, 0, 1), side, side));
            faces.Add(new Face(new Vertex(center.x, center.y - side / 2, center.z), new Vector(0, -1, 0), new Vector(0, 0, 1), side, side));
            faces.Add(new Face(new Vertex(center.x + side / 2, center.y, center.z), new Vector(1, 0, 0), new Vector(0, 1, 0), side, side));
            faces.Add(new Face(new Vertex(center.x - side / 2, center.y, center.z), new Vector(-1, 0, 0), new Vector(0, 1, 0), side, side));
        }

        public override Tuple<Vertex, Vector> getIntersection(Vector direction, Vertex origin)
        {
            double nearestVertex = double.MaxValue;
            Tuple<Vertex, Vector> res = null;
            foreach (var face in faces)
            {
                Tuple<Vertex, Vector> intersectionAndNormale;
                if ((intersectionAndNormale = face.getIntersection(direction, origin)) != null && MyMath.Distance(origin, intersectionAndNormale.Item1) < nearestVertex)
                {
                    nearestVertex = MyMath.Distance(origin, intersectionAndNormale.Item1);
                    res = intersectionAndNormale;
                }
            }
            return res;
        }
    }

    class Room : Shape
    {
        double side;
        private List<Face> faces;
        public Room(Vertex center, double side, Face leftWall, Face rightWall, Face backWall, Face floor, Face ceiling, Face cameraWall)
        {
            this.center = center;
            this.side = side;
            faces = new List<Face>();

            faces.Add(cameraWall);
            faces.Add(backWall);
            faces.Add(ceiling);
            faces.Add(floor);
            faces.Add(rightWall);
            faces.Add(leftWall);
        }

        public override Tuple<Vertex, Vector> getIntersection(Vector direction, Vertex origin)
        {
            double nearestVertex = double.MaxValue;
            Tuple<Vertex, Vector> res = null;
            foreach (var face in faces)
            {
                Tuple<Vertex, Vector> intersectionAndNormale;
                if ((intersectionAndNormale = face.getIntersection(direction, origin)) != null)
                {
                    nearestVertex = intersectionAndNormale.Item1.z;
                    res = intersectionAndNormale;
                    color = face.color;
                    material = face.material;
                }
            }
            return res;
        }
    }

    abstract class Shape
    {
        public Vertex center;
        public Color color;
        public Material material;

        public abstract Tuple<Vertex, Vector> getIntersection(Vector direction, Vertex origin);
    }

    class Face : Shape
    {
        double width;
        double height;
        public Vector normal;
        public Vector heightVector, widthVector;

        public Face(Vertex center, Vector normal, Vector heightVector, double width, double height)
        {
            this.center = center;
            this.width = width;
            this.height = height;
            this.normal = normal;
            this.heightVector = heightVector.normalize();
            this.widthVector = (normal * heightVector).normalize();
        }

        public Face(Vertex center, Vector normal, Vector heightVector, double width, double height, Color color, Material material)
        {
            this.color = color;
            this.center = center;
            this.width = width;
            this.height = height;
            this.normal = normal;
            this.material = material;
            this.heightVector = heightVector.normalize();
            this.widthVector = (normal * heightVector).normalize();
        }

        public Vertex worldToFaceBasis(Vertex p)
        {
            return new Vertex(
                widthVector.x * (p.x - center.x) + widthVector.y * (p.y - center.y) +
                widthVector.z * (p.z - center.z),
                heightVector.x * (p.x - center.x) + heightVector.y * (p.y - center.y) +
                heightVector.z * (p.z - center.z),
                normal.x * (p.x - center.x) + normal.y * (p.y - center.y) +
                normal.z * (p.z - center.z));
        }

        public override Tuple<Vertex, Vector> getIntersection(Vector direction, Vertex origin)
        {
            if (Math.Abs(normal.x * (origin.x - center.x) + normal.y * (origin.y - center.y) + normal.z * (origin.z - center.z)) < 0.00001) // точка выпуска луча лежит в плоскости
            {
                return null;
            }

            double tn = -(normal.x * center.x) - (normal.y * center.y) - (normal.z * center.z) + (normal.x * origin.x) + (normal.y * origin.y) + (normal.z * origin.z);
            if (Math.Abs(tn) < 0.00001)          // прямая лежит на плоскости, если знаменатель тоже 0, не интересно
            {
                return null;
            }
            double td = normal.x * direction.x + normal.y * direction.y + normal.z * direction.z;
            if (Math.Abs(td) < 0.00001)          // прямая параллельна плоскости, не интересно
            {
                return null;
            }
            var pointInWorld = MyMath.VertexOnLine(origin, direction, -tn / td);
            if (new Vector(origin, pointInWorld).z / direction.z < 0)     // вектор из полученной точки в ориджин всегда коллинеарен направлению, 
            {                                                            // но если х1/x2=y1/у2=z1/z2 - отрицательны, они противонаправлены
                return null;
            }
            var pointOnSurface = worldToFaceBasis(pointInWorld);

            if (Math.Abs(pointOnSurface.x) <= width / 2 && Math.Abs(pointOnSurface.y) <= height / 2)
            {
                return Tuple.Create(pointInWorld, normal);
            }
            return null;
        }
    }

    class Vector
    {
        public double x, y, z;

        public Vector(double x, double y, double z, bool isVectorNeededToBeNormalized = false)
        {
            double normalization = isVectorNeededToBeNormalized
                ? Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2))
                : 1;
            this.x = x / normalization;
            this.y = y / normalization;
            this.z = z / normalization;
        }

        public Vector(Vertex p, bool isVectorNeededToBeNormalized = false) : this(p.x, p.y, p.z,
            isVectorNeededToBeNormalized)
        {
        }

        public Vector(Vertex start, Vertex end, bool isVectorNeededToBeNormalized = false) : this(end.x - start.x, end.y - start.y, end.z - start.z, isVectorNeededToBeNormalized) { }

        public Vector normalize()
        {
            double normalization = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            x = x / normalization;
            y = y / normalization;
            z = z / normalization;
            return this;
        }

        public static Vector operator -(Vector v1, Vector v2) => new Vector(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        public static Vector operator +(Vector v1, Vector v2) => new Vector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        public static Vector operator *(Vector a, Vector b) => new Vector(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        public static Vector operator *(double k, Vector b) => new Vector(k * b.x, k * b.y, k * b.z);
        public static double operator ^(Vector a, Vector b) => a.x * b.x + a.y * b.y + a.z * b.z;

    }

    class Vertex
    {
        public double x, y, z;

        public Vertex(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }


    }

    class Material
    {
        public double shininess;
        public double kspecular;
        public double kdiffuse;
        public double kambient;

        public double transparency;
        public double reflectivity;

        public Material(double shininess, double kspecular, double kdiffuse, double kambient, double transparency, double reflectivity)
        {
            this.shininess = shininess;
            this.kspecular = kspecular;
            this.kdiffuse = kdiffuse;
            this.kambient = kambient;
            this.transparency = transparency;
            this.reflectivity = reflectivity;
        }
    }

    class LightSource
    {
        public Vertex location;
        public double intensity;
        public Color color;

        public LightSource(Vertex location, double intensivity, Color color)
        {
            this.location = location;
            this.intensity = intensivity;
            this.color = color;
        }
    }

    public class Matrix
    {
        double[,] matr;
        int rowCount;
        int colCount;

        public Matrix(int rows, int cols)
        {
            rowCount = rows;
            colCount = cols;
            matr = new double[rows, cols];
        }

        public Matrix fill(params double[] elems)
        {
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    matr[i, j] = elems[i * colCount + j];
                }
            }
            return this;
        }

        public Matrix fillAffine(params double[] elems)
        {
            return fill(elems[0], elems[1], 0, elems[2], elems[3], 0, elems[4], elems[5], 1);
        }

        public double this[int x, int y]
        {
            get
            {
                return matr[x, y];
            }
            set
            {
                matr[x, y] = value;
            }
        }

        public static Matrix operator *(Matrix matr, double value)
        {
            var res = new Matrix(matr.rowCount, matr.colCount);
            for (int i = 0; i < matr.rowCount; i++)
            {
                for (int j = 0; j < matr.colCount; j++)
                {
                    res[i, j] = matr[i, j] * value;
                }
            }
            return res;
        }

        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            if (matrix1.colCount != matrix2.rowCount)
            {
                throw new Exception("Так умножать нельзя...");
            }
            var res = new Matrix(matrix1.rowCount, matrix2.colCount);
            for (int i = 0; i < res.rowCount; i++)
            {
                for (int j = 0; j < res.colCount; j++)
                {
                    for (var k = 0; k < matrix1.colCount; k++)
                    {
                        res[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                }
            }
            return res;
        }
    }

    class MyMath
    {
        public static Vertex GetVertexProjection(Vertex origin, Vector direction, Vertex projected)
        {
            double parameter = (direction.x * (projected.x - origin.x) + direction.y * (projected.y - origin.y) + direction.z * (projected.z - origin.z)) / (Math.Pow(direction.x, 2) + Math.Pow(direction.y, 2) + Math.Pow(direction.z, 2));
            return VertexOnLine(origin, direction, parameter);
        }

        public static Vertex VertexOnLine(Vertex origin, Vector direction, double parameter)
        {
            return new Vertex(origin.x + direction.x * parameter, origin.y + direction.y * parameter, origin.z + direction.z * parameter);
        }

        public static double Distance(Vertex v1, Vertex v2)
        {
            return Math.Sqrt(Math.Pow(v2.x - v1.x, 2) + Math.Pow(v2.y - v1.y, 2) + Math.Pow(v2.z - v1.z, 2));
        }

        public static double DegsToRads(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;
            return value;
        }
    }
}
