using UnityEngine;  
using UnityEditor;  
using IronPython;  
using IronPython.Modules;  
using System.Text;  
using System.Collections.Generic;  
using Microsoft.Scripting.Hosting;  

public class PythonTools : EditorWindow  {
	//source: http://techartsurvival.blogspot.com/2013/12/techartists-doin-it-for-themselves.html
	//dependency: IronPython (see https://github.com/exodrifter/unity-python)
	//updated by RMKD for Unity 5.5

	// class member properties
	Vector2 _historyScroll;  
	Vector2 _scriptScroll;  
	bool _showHistory = true;  
	int _historyPaneHeight = 192;  
	string _historyText = "history";  
	string _scriptText = "script";  
	string _lastResult = "";  
	TextEditor _TEditor;  
	GUIStyle consoleStyle = new GUIStyle ();  
	GUIStyle historyStyle = new GUIStyle ();  
	Microsoft.Scripting.Hosting.ScriptEngine _ScriptEngine;  
	Microsoft.Scripting.Hosting.ScriptScope _ScriptScope;  

	// Add menu named "My Window" to the Window menu
	[MenuItem ("Python/REPL")]
	static void Init () {		
		// Get existing open window or if none, make a new one:
		PythonTools window = (PythonTools)EditorWindow.GetWindow (typeof (PythonTools));
		window.Show();
	}

	// initialization logic (it's Unity, so we don't do this in the constructor!
	public void OnEnable ()  { 

		PythonTools window = (PythonTools)EditorWindow.GetWindow (typeof (PythonTools));
		window.titleContent = new GUIContent ("Python Console", "Execute Unity functions via IronPython");

		// pure gui stuff
		consoleStyle.normal.textColor = Color.cyan;  
		consoleStyle.margin = new RectOffset (20, 10, 10, 10);  
		historyStyle.normal.textColor = Color.white;  
		historyStyle.margin = new RectOffset (20, 10, 10, 10);  

		// load up the hosting environment  
		_ScriptEngine = IronPython.Hosting.Python.CreateEngine ();  
		_ScriptScope = _ScriptEngine.CreateScope ();  

		// load the assemblies for unity, using types  
		// to resolve assemblies so we don't need to hard code paths  
		_ScriptEngine.Runtime.LoadAssembly (typeof(PythonFileIOModule).Assembly);  
		_ScriptEngine.Runtime.LoadAssembly (typeof(GameObject).Assembly);  
		_ScriptEngine.Runtime.LoadAssembly (typeof(Editor).Assembly);  
		string dllpath = System.IO.Path.GetDirectoryName (  
			(typeof(ScriptEngine)).Assembly.Location).Replace (  
				"\\", "/");  
		// load needed modules and paths  
		StringBuilder init = new StringBuilder ();  
		init.AppendLine ("import sys");  
		init.AppendFormat ("sys.path.append(\"{0}\")\n", dllpath + "/Lib");  
		init.AppendFormat ("sys.path.append(\"{0}\")\n", dllpath + "/DLLs");  
		init.AppendLine ("import UnityEngine as unity");  
		init.AppendLine ("import UnityEditor as editor");  
		init.AppendLine ("from cStringIO import StringIO");  
		init.AppendLine ("unity.Debug.Log(\"Python console initialized\")");  
		init.AppendLine ("__print_buffer = sys.stdout = StringIO()");  
		var ScriptSource = _ScriptEngine.CreateScriptSourceFromString (init.ToString ());  
		ScriptSource.Execute (_ScriptScope);  
	}   
		
	public void OnGUI ()  {  
		HackyTabSubstitute ();  // this is explained below...

		// top pane with history  
		_showHistory = EditorGUILayout.Foldout (_showHistory, "History");  
		if (_showHistory) {  
			EditorGUILayout.BeginVertical (GUILayout.ExpandWidth (true),   
				GUILayout.Height (_historyPaneHeight));  
			if (GUILayout.Button ("Clear history")) {  
				_historyText = "";  
			}  
			_historyScroll = EditorGUILayout.BeginScrollView (_historyScroll);  
			EditorGUILayout.TextArea (_historyText,   
				historyStyle,   
				GUILayout.ExpandWidth (true),   
				GUILayout.ExpandHeight (true));          
			EditorGUILayout.EndScrollView ();  
			EditorGUILayout.EndVertical ();  
		}  
		// draggable splitter  
		GUILayout.Box ("", GUILayout.Height (8), GUILayout.ExpandWidth (true));  
		//Lower pane for script editing  
		EditorGUILayout.BeginVertical (GUILayout.ExpandWidth (true),   
			GUILayout.ExpandHeight (true));  
		_scriptScroll = EditorGUILayout.BeginScrollView (_scriptScroll);  
		GUI.SetNextControlName ("script_pane");  
		// note use of GUILayout NOT EditorGUILayout.  
		// TextEditor is not accessible for EditorGUILayout!  
		_scriptText = GUILayout.TextArea (_scriptText,   
			consoleStyle,  
			GUILayout.ExpandWidth (true),   
			GUILayout.ExpandHeight (true));          
		_TEditor = (TextEditor)GUIUtility.GetStateObject (typeof(TextEditor), GUIUtility.keyboardControl);  
		EditorGUILayout.EndScrollView ();  
		EditorGUILayout.BeginHorizontal ();  
		if (GUILayout.Button("Clear", GUILayout.ExpandWidth(true)))  
		{  
			_scriptText = "";  
			GUI.FocusControl("script_pane");  
		}  
		if (GUILayout.Button ("Execute and clear", GUILayout.ExpandWidth (true))) {  
			Intepret (_scriptText);  
			_scriptText = "";  
			GUI.FocusControl("script_pane");  
		}  
		if (GUILayout.Button ("Execute", GUILayout.ExpandWidth (true))) {  
			Intepret (_scriptText);  
		}  
		EditorGUILayout.EndHorizontal ();  
		EditorGUILayout.EndVertical ();      
		// mimic maya Ctrl+enter = execute  
		if (Event.current.isKey &&  
			Event.current.keyCode == KeyCode.Return &&  
			Event.current.type == EventType.KeyUp &&  
			Event.current.control) {  
			Intepret (_scriptText);  
		}  
		// drag the splitter  
		if (Event.current.isMouse & Event.current.type == EventType.mouseDrag)  
		{_historyPaneHeight = (int) Event.current.mousePosition.y - 28;  
			Repaint();  
		}  
	}

	private void HackyTabSubstitute ()  {  
		string _t = _scriptText;  
		string[] lines = _scriptText.Split ('\n');  
		for (int i = 0; i< lines.Length; i++) {  
			if (lines [i].IndexOf ('`') >= 0) {  
				lines [i] = "  " + lines [i];  
				_TEditor.cursorIndex = _TEditor.cursorIndex = _TEditor.cursorIndex + 3;
			}  
			if (lines [i].IndexOf ("  ") >= 0 && lines [i].IndexOf ("~") >= 0) {  
				if (lines [i].StartsWith ("  "))  
					lines [i] = lines [i].Substring (4);  
				_TEditor.cursorIndex = _TEditor.cursorIndex = _TEditor.cursorIndex - 4;
			}  
			lines [i] = lines [i].Replace ("~", "");  
			lines [i] = lines [i].Replace ("`", "");  
		}  
		_scriptText = string.Join ("\n", lines);  
		if (_scriptText != _t)  
			Repaint ();  
	}  

	// Pass the script text to the interpreter and display results  
	private void Intepret (string text_to_interpret)  {  
		object result = null;  
		try {  
			Undo.RegisterSceneUndo ("script");  
			var scriptSrc = _ScriptEngine.CreateScriptSourceFromString (text_to_interpret);  
			_historyText += "\n";  
			_historyText += text_to_interpret;  
			_historyText += "\n";  
			result = scriptSrc.Execute (_ScriptScope);  
		}   
		// Log exceptions to the console too  
		catch (System.Exception e) {  
			Debug.LogException (e);  
			_historyText += "\n";  
			_historyText += "#  " + e.Message + "\n";  
		}   
		finally {  
			// grab the __print_buffer stringIO and get its contents  
			var print_buffer = _ScriptScope.GetVariable ("__print_buffer");  
			var gv = _ScriptEngine.Operations.GetMember (print_buffer, "getvalue");  
			var st = _ScriptEngine.Operations.Invoke (gv);  
			var src = _ScriptEngine.CreateScriptSourceFromString ("__print_buffer = sys.stdout = StringIO()");  
			src.Execute (_ScriptScope);  
			if (st.ToString ().Length > 0) {  
				_historyText += "";  
				foreach (string l in st.ToString().Split('\n'))  
				{  
					_historyText += "  " + l + "\n";  
				}  
				_historyText += "\n";  
			}  
			// and print the last value for single-statement evals  
			if (result != null) {  
				_historyText += "#  " + result.ToString () + "\n";  
			}  
			int lines = _historyText.Split ('\n').Length;  
			_historyScroll.y += (lines * 19);                  
			Repaint ();  
		}  
	}  

	[MenuItem("Python/HelloWorldRuntime")]  
	public static void UnityScriptTest()  {

		//source: http://techartsurvival.blogspot.com/2013/12/techartists-doin-it-for-themselves.html

		// create the engine like last time  
		var ScriptEngine = IronPython.Hosting.Python.CreateEngine();  
		var ScriptScope = ScriptEngine.CreateScope();  
		// load the assemblies for unity, using the types of GameObject  
		// and Editor so we don't have to hardcoded paths  
		ScriptEngine.Runtime.LoadAssembly(typeof(GameObject).Assembly);  
		ScriptEngine.Runtime.LoadAssembly(typeof(Editor).Assembly);  
		StringBuilder example = new StringBuilder();  
		example.AppendLine("import UnityEngine as unity");  
		example.AppendLine("import UnityEditor as editor");  
		example.AppendLine("unity.Debug.Log(\"hello from inside the editor\")");  
		var ScriptSource = ScriptEngine.CreateScriptSourceFromString(example.ToString());  
		ScriptSource.Execute(ScriptScope);  
	}

	[MenuItem("Python/Numpy Test")]  
	public static void NumpyTest()  {
		var ScriptEngine = IronPython.Hosting.Python.CreateEngine();  
		var ScriptScope = ScriptEngine.CreateScope();  
		ScriptEngine.Runtime.LoadAssembly(typeof(GameObject).Assembly);  
		ScriptEngine.Runtime.LoadAssembly(typeof(Editor).Assembly);  
		StringBuilder example = new StringBuilder();  
		example.AppendLine("import UnityEngine as unity");  
		example.AppendLine("import UnityEditor as editor"); 
		example.AppendLine("import numpy as np"); 
		var ScriptSource = ScriptEngine.CreateScriptSourceFromString(example.ToString());  
		ScriptSource.Execute(ScriptScope);  
	}
}  

  
