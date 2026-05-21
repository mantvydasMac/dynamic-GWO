namespace GWO.Util;

public class Vector
{
    private readonly double[] _array;

    public Vector(int size)
    {
        _array = new double[size];
    }

    public Vector(double[] array)
    {
        _array = array;
    }

    public int Size()
    {
        return _array.Length;
    }
    
    public double Get(int index)
    {
        return _array[index];
    }

    public void Set(int index, double value)
    {
        _array[index] = value;
    }

    public void Clamp(Bounds bounds)
    {
        for (int i = 0; i < Size(); ++i)
        {
            if (Get(i) > bounds.Upper)
            {
                Set(i, bounds.Upper);
            }
            else if (Get(i) < bounds.Lower)
            {
                Set(i, bounds.Lower);
            }
        }
    }

    public Vector Clone()
    {
        var newArray = new double[Size()];

        for(int i = 0;i<Size();++i)
        {
            newArray[i] = _array[i];
        }

        return new Vector(newArray);
    }

    public void Print()
    {
        Console.Write("[ ");
        for(int i = 0;i<Size()-1;++i) 
        {
            Console.Write($" {_array[i]} |");
        }
        Console.WriteLine($" {_array[Size()-1]} ]");
    }

    public Vector Abs()
    {
        double[] arr = new double[Size()];

        for (int i = 0; i < Size(); ++i)
        {
            arr[i] = Math.Abs(Get(i));
        }

        return new Vector(arr);
    }
    
    public static double EuclideanDistance(Vector a, Vector b)
    {
        if(a.Size() != b.Size()) throw new ArithmeticException("Vectors not same length");
        
        double sum = 0.0;

        for (int i = 0; i < a.Size(); i++)
        {
            double diff = a.Get(i) - b.Get(i);
            sum += diff * diff;
        }

        return Math.Sqrt(sum);
    }
    
    // OPERATORS
    public static Vector operator +(Vector operand) => operand;
    
    public static Vector operator -(Vector operand)
    {
        double[] arr = new double[operand.Size()];

        for (int i = 0; i < operand.Size(); ++i)
        {
            arr[i] = -operand.Get(i);
        }

        return new Vector(arr);
    }
    
    public static Vector operator +(Vector left, Vector right)
    {
        if (left.Size() != right.Size()) throw new ArithmeticException("Vectors not same length");

        double[] arr = new double[left.Size()];

        for (int i = 0; i < left.Size(); ++i)
        {
            arr[i] = left.Get(i) + right.Get(i);
        }

        return new Vector(arr);
    }

    public static Vector operator +(Vector vector, double number)
    {
        double[] arr = new double[vector.Size()];

        for (int i = 0; i < vector.Size(); ++i)
        {
            arr[i] = vector.Get(i) + number;
        }

        return new Vector(arr);
    }

    public static Vector operator +(double number, Vector vector) => vector + number;
    
    public static Vector operator -(Vector left, Vector right) => left + (-right);
    public static Vector operator -(Vector vector, double number) => vector + (-number);

    public static Vector operator *(Vector left, Vector right)
    {
        if (left.Size() != right.Size()) throw new ArithmeticException("Vectors not same length");

        double[] arr = new double[left.Size()];

        for (int i = 0; i < left.Size(); ++i)
        {
            arr[i] = left.Get(i) * right.Get(i);
        }

        return new Vector(arr);
    }

    public static Vector operator *(Vector vector, double number)
    {
        double[] arr = new double[vector.Size()];

        for (int i = 0; i < vector.Size(); ++i)
        {
            arr[i] = vector.Get(i) * number;
        }

        return new Vector(arr);
    }
    
    public static Vector operator *(double number, Vector vector) => vector * number;
    
    public static Vector operator /(Vector vector, double number)
    {
        double[] arr = new double[vector.Size()];

        for (int i = 0; i < vector.Size(); ++i)
        {
            arr[i] = vector.Get(i) / number;
        }

        return new Vector(arr);
    }
}