using UnityEngine;
using System;
using System.Collections;
using OpenNI;


#pragma warning disable 0414		//warning about not using scriptNode

public class OpenNIContext
{
	private static OpenNIContext instance;

    public string OpenNIXMLFilename = ".\\OpenNI.xml";
    public Context context;
    public DepthGenerator Depth;
    private MirrorCapability mirror;

    static private bool validContext = false;


    public static OpenNIContext Instance()
    {
        if (instance == null)
        {
            instance = new OpenNIContext();
            if (!validContext)
                instance = null;
        }

        return instance;
    }

	OpenNIContext()
	{
		MonoBehaviour.print("normal constructor");
		Init();
	}

	~OpenNIContext()
	{
		MonoBehaviour.print("Destroying context");
	}
	
	public bool Mirror
	{
		get { return mirror.IsMirrored(); }
		set { mirror.SetMirror(value); }
	}
	
	static public bool ValidContext()
	{
		return validContext;
	}
	
    private ScriptNode scriptNode;

	private void Init()
	{
        Debug.Log("Creating Context");

        try
        {
            //this.context = new OpenNI.Context(OpenNIXMLFilename);
            this.context = Context.CreateFromXmlFile(OpenNIXMLFilename, out scriptNode);
        }
        catch (OpenNI.GeneralException ex)
        {
            Debug.Log("Context not created: ");
            Debug.Log(ex.Message);
            return;
        }

        //this.context = new OpenNI.Context(OpenNIXMLFilename);
        this.context = Context.CreateFromXmlFile(OpenNIXMLFilename, out scriptNode);
        this.Depth = new DepthGenerator(this.context);
        this.mirror = this.Depth.MirrorCapability;

        MonoBehaviour.print("OpenNI inited");

        validContext = true;

      	this.context.StartGeneratingAll();
	}
	
	// Update is called once per frame
	public void Update () 
	{
		if (validContext)
		{
			this.context.WaitNoneUpdateAll();
		}
	}

	public void Restart()
	{
		this.context.StopGeneratingAll();
		this.context.Release();
		OpenNIContext.validContext = false;
		this.context = null;

		OpenNIContext.instance = null;
	}
}