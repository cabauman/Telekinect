using UnityEngine;
using System.Collections;

using System;
using System.Runtime.InteropServices;


//Singleton Class to wrap implementation of RNGenerator
public class RNG
{
    //C Functions from DLL
        [DllImport("RNGeneratorDLL.dll")]
        static private extern IntPtr CreateRNGenerator();

        [DllImport("RNGeneratorDLL.dll")]
        static private extern void DeleteRNGenerator(IntPtr pRNG);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern void SetSeed(IntPtr pRNG, uint nSeed);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern uint GetSeed(IntPtr pRNG);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern float fGenerate(IntPtr pRNG);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern double dGenerate(IntPtr pRNG);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern float fUniform(IntPtr pRNG, float a, float b);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern double dUniform(IntPtr pRNG, double a, double b);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern float fExponential(IntPtr pRNG, float B);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern double dExponential(IntPtr pRNG, double B);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern float fWeibull(IntPtr pRNG, float a, float b, float c);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern double dWeibull(IntPtr pRNG, double a, double b, double c);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern float fTriangle(IntPtr pRNG, float Xmin, float Xmax, float c);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern double dTriangle(IntPtr pRNG, double Xmin, double Xmax, double c);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern float fNormal(IntPtr pRNG, float mean);

        [DllImport("RNGeneratorDLL.dll")]
        static private extern double dNormal(IntPtr pRNG, double mean);

    //Data members
        private IntPtr pRNG;
        private static RNG instance;

    //Methods
        private RNG() //private constructor - singleton
        {
            pRNG = CreateRNGenerator();
        }

        ~RNG()
        {

        }

        public static RNG Instance()
        {
            if (instance == null)
            {
                instance = new RNG();
            }

            return instance;
        }

        //Wrapper Methods

            public void ResetSeed(uint nSeed)
            {
                SetSeed(this.pRNG, nSeed);
            }

            public uint CurrentSeed()
            {
                return GetSeed(this.pRNG);
            }

            public float fGen()
            {
                return fGenerate(pRNG);
            }

            public double dGen()
            {
                return dGenerate(pRNG);
            }

            public float fUni(float a, float b)
            {
                return fUniform(pRNG, a, b);
            }

            public double dUni(double a, double b)
            {
                return dUniform(pRNG, a, b);
            }

            public float fExp(float B)
            {
                return fExponential(pRNG, B);
            }

            public double dExp(double B)
            {
                return dExponential(pRNG, B);
            }

            public float fWei(float a, float b, float c)
            {
                return fWeibull(pRNG, a, b, c);
            }

            public double dWei(double a, double b, double c)
            {
                return dWeibull(pRNG, a, b, c);
            }

            public float fTri(float Xmin, float Xmax, float c)
            {
                return fTriangle(pRNG, Xmin, Xmax, c);
            }

            public double dTri(double Xmin, double Xmax, double c)
            {
                return dTriangle(pRNG, Xmin, Xmax, c);
            }

            public float fNorm(float mean)
            {
                return fNormal(pRNG, mean);
            }

            public double dNorm(double mean)
            {
                return dNormal(pRNG, mean);
            }
}