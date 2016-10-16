using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

namespace D3Sharp {
	/* Requires https://github.com/lbv/litjson (built with v0.9.0)
	   usage: declare a d3 Element, then copy and paste d3 code into your Unity c# scripts
	  	var d3 = new Element (this.gameObject);
		var svg = d3.append ("svg")
			.attr ("width",800)
			.attr ("height", 600);
			
		svg.selectAll("text").append("text")
			.attr("x",250)
			.attr("y",10)
			.text ("hello D3Sharp");
		
		svg.selectAll("rect").append ("rect").attr("fill","#ff0000")
			.attr("x",400)
			.attr("y",100)
			.attr("width",400)
			.attr("height",150);
		
		svg.selectAll("circle").append ("circle")
			.attr ("fill", "#78f")
			.attr("cx",400)
			.attr("cy",300);			
	*/

	class Element {
		public float meterPerPixelRatio = 0.01f; 
		List<Element> current;
		GameObject gameObject;
		string type;
		string parentType;
		JsonData __data__;
		Sprite circle = Resources.Load<Sprite> ("white-circle-md");
		Font defaultFont = Resources.GetBuiltinResource (typeof(Font), "Arial.ttf") as Font;

		public static void UseHTMLStyleCoordinates(GameObject go, bool centered=false){
			RectTransform rt = go.GetComponent<RectTransform> ();
			rt.anchoredPosition = Vector2.zero;
			rt.anchorMin = new Vector2 (0, 1);
			rt.anchorMax = new Vector2 (0, 1);
			if(!centered) rt.pivot = new Vector2 (0, 1);
		}

		public Element(GameObject parent){
			this.current = new List<Element>();
			this.gameObject = new GameObject();
			this.gameObject.transform.SetParent(parent.transform);
			this.gameObject.name = "";
			this.__data__ = JsonMapper.ToObject (@"{}");
		}

		public Element select(string sel){
			Debug.Log ("Select is not working correctly"); //TODO fix
			//section adds elements to the current list
			foreach(Element e in current){
				Debug.Log ("SELECT ONE " + sel + " " + e.gameObject.name +" count:"+current.Count);
				if(e.gameObject.name.Contains(sel)){
					Debug.Log("Match on "+sel + " in "+e.gameObject.name);
				}
			}
			current = new List<Element>();
			current.Add(new Element(this.gameObject));
			return this;
		}

		public Element selectAll(string sel){
			//section adds elements to the current list
			return select(sel);//TODO write function to handle multiples
		}

		public Element enter(){
			return this;
		}

		public Element data(JsonData data){
			return this;
		}

		public Element datum(JsonData data){
			return this;
		}

		public Element append(string element){
			
			if (current.Count == 0) {
				if (element == "g" ) {
					Element g = new Element (this.gameObject);
					handleAppend (g.gameObject, element);
					return g;
				} else if (this.gameObject.name.StartsWith ("g") || this.gameObject.name.StartsWith("svg")){
					//this is hte case of the first child of a group
					Element e = new Element (this.gameObject);
					handleAppend (e.gameObject, element);
					return e;
				} else if (element == "svg") {
					handleAppend (this.gameObject, element);
				} else {
					Debug.LogError (string.Format("{0} has NO CURRENT CHILDREN of {1}", this.gameObject.name, element));
				}
			} else {
				foreach (Element selection in current) {
					handleAppend (selection.gameObject, element);
				}
			}
			return this;
		}

		private void handleAppend(GameObject go, string element){
			Image img;
			Text t;
			go.name += element;
			switch (element) {
			case "svg":
				Canvas c = this.gameObject.AddComponent<Canvas> () as Canvas;
				c.renderMode = RenderMode.WorldSpace;
				this.gameObject.AddComponent<CanvasScaler> ();
				this.gameObject.AddComponent<GraphicRaycaster> ();
				UseHTMLStyleCoordinates (this.gameObject);
				break;
			case "g":
				go.AddComponent<RectTransform>();
				UseHTMLStyleCoordinates (go);
				break;
			case "rect":
				img = go.AddComponent<Image> ();
				UseHTMLStyleCoordinates (go);
				break;
			case "circle":
				img = go.AddComponent<Image> ();
				UseHTMLStyleCoordinates (go, true);
				img.sprite = circle;
				break;
			case "path":
			case "line":
				go.name += " (wip)";
				go.AddComponent<RectTransform> ();
				UseHTMLStyleCoordinates (go);
				LineRenderer lr = go.AddComponent<LineRenderer> ();
				lr.material.SetColor ("_Color", Color.blue);
				lr.useWorldSpace = false;
				lr.numPositions = 2;
				lr.SetPosition (0, Vector3.zero);

				break;
			case "text":
				t = go.AddComponent<Text> ();
				UseHTMLStyleCoordinates (go);
				t.font = defaultFont;
				t.fontStyle = FontStyle.Bold;
				t.color = Color.black;
				break;
			case "button":
				go.name += " (wip)";
				Button b = go.AddComponent<Button> () as Button;
				b.gameObject.AddComponent<RectTransform> ();
				UseHTMLStyleCoordinates (go);
				img = b.gameObject.AddComponent < Image> ();
				img.sprite = circle; //TODO replace with a button image
				GameObject child = new GameObject ();
				t = child.gameObject.AddComponent<Text> ();
				t.font = defaultFont;
				t.fontStyle = FontStyle.Bold;
				t.color = Color.black;
				child.transform.SetParent (b.transform);
				break;
			default:
				go.name += " (unsupported)";
				Debug.Log (element + " elements are not yet supported");
				break;
			}


		}


		public Element attr(string k, float v){
			if (current.Count == 0) {
				handleAttr (this.gameObject, k, v);
			} else {
				foreach (Element selection in current) {
					handleAttr (selection.gameObject, k, v);
				}
			}
			return this;
		}

		public Element attr(string k, string v){
			if (current.Count == 0) {
				handleAttr (this.gameObject, k, v);
			} else {
				foreach (Element selection in current) {
					handleAttr (selection.gameObject, k, v);
				}
			}
			return this;
		}

		public Element text(string t){
			if (current.Count == 0) {
				handleText (this.gameObject, t);
			} else {
				foreach (Element selection in current) {
					handleText (selection.gameObject, t);
				}
			}
			return this;
		}

		private void handleText(GameObject go, string t){
			Text textElement = go.GetComponent<Text> ();
			Button buttonElement = go.GetComponent<Button> ();
			if (textElement) {
				textElement.text = t;
			}
			else if (buttonElement) {
				buttonElement.GetComponentInChildren<Text> ().text = t;
			}
		}

		//handle numerical attributes
		private void handleAttr(GameObject go, string k, float v){
			RectTransform rt = go.GetComponent<RectTransform> ();
			if (rt == null) {
				go.AddComponent<RectTransform> ();
			}
			switch (k) {
			case "x":
			case"cx":
				rt.anchoredPosition = new Vector2(v, rt.anchoredPosition.y);
				break;
			case "y":
			case "cy":
				rt.anchoredPosition = new Vector2 (rt.anchoredPosition.x, -v); //NOTE manually reversed
				break;
			case "width":
				rt.sizeDelta = new Vector2 (v, rt.sizeDelta.y);
				break;
			case "height":
				rt.sizeDelta = new Vector2 (rt.sizeDelta.x, v);
				break;
			case "x2":
				Debug.Log ("RENDER LINE" + k + " " + v);
				LineRenderer lr = go.GetComponent<LineRenderer> ();
				Vector3 p = lr.GetPosition (1);
				lr.SetPosition (1, new Vector3 (v, p.y, p.z));
				break;
			case "y2":
				LineRenderer lr2 = go.GetComponent<LineRenderer> ();
				Vector3 p2 = lr2.GetPosition (1);
				lr2.SetPosition (1, new Vector3 (v, p2.y, p2.z));
				break;
			case "rotate-z":
				Debug.Log ("ROTATION" + go.name + " " + v);
				rt.localRotation = Quaternion.Euler (new Vector3 (rt.localRotation.x, rt.localRotation.y, -v));//NOTE manually reversed
				break;
			default:
				Debug.Log ("Numerical attr " + k + " not recognized");
				break;
			}
		}

		//handle string attributes
		private void handleAttr(GameObject go, string k, string v){
			Image img = go.GetComponent<Image> ();
			switch (k) {
			case "fill":
				if (!img) {
					go.AddComponent<Image> ();
				}
				Color c = new Color ();
				ColorUtility.TryParseHtmlString (v, out c);
				img.color = c;
				break;
			case "id":
				go.name += "#" + v;
				break;
			case "class":
				go.name += "." + v;
				break;
			case "width":
			case "height":
			case "x2":
			case "y2":
				handleAttr (go, k, float.Parse (v));
				break;
			case "transform":
				HandleTransform (go, v);
				break;
			case "text-anchor":
				Debug.Log ("text-anchor not working");
				break;
				Text t = go.GetComponent<Text> ();
				switch (v) {
				case "end":
					t.alignment = TextAnchor.LowerRight;
					break;
				case "middle":
					t.alignment = TextAnchor.LowerCenter;
					break;
				default:
					t.alignment = TextAnchor.LowerLeft;
					break;
				}
				break;
			default:
				Debug.Log ("attr " + k + " not recognized");
				break;
			}
		}

		public void HandleTransform(GameObject go, string transformText){
			//handle translation
			Regex translate = new Regex (@"translate\(([0-9., \-]+)\)");
			Match translateMatch = translate.Match (transformText);
			if (translateMatch.Success) {
				string[] vals = translateMatch.Groups[1].Value.Split (',');
				float x = 0;
				float y = 0;
				float.TryParse(vals[0], out x);
				float.TryParse (vals [1], out y);
				RectTransform rt = go.GetComponent<RectTransform> ();
				rt.anchoredPosition = new Vector2(x, -y);
			}

			//handle rotation
			Regex rotate = new Regex (@"rotate\(([0-9., ]+)\)");
			Match rotateMatch = rotate.Match (transformText);
			if (rotateMatch.Success) {
				string[] vals = rotateMatch.Groups[1].Value.Split (',');

				float z = 0;
				float.TryParse(vals[0], out z);
				handleAttr (go, "rotate-z", z);
			}

			//TODO handle scale

			//HACK to handle secondary translation 
			if (translateMatch.Success && translateMatch.Groups.Count > 2) {
				string[] vals = translateMatch.Groups[2].Value.Split (',');
				float x = 0;
				float y = 0;
				float.TryParse(vals[0], out x);
				float.TryParse (vals [1], out y);
				RectTransform rt = go.GetComponent<RectTransform> ();
				rt.anchoredPosition = new Vector2(rt.anchoredPosition.x + x, rt.anchoredPosition.y - y);
			}
		}
			
		public static jsFunction function(string code){
			return new jsFunction (code);
		}
			
		public Element attr(string k, jsFunction f){
			Debug.Log ("functions are not yet supported for " + k);
			for(var i=0; i < current.Count; i++){
				f.execute (JsonMapper.ToObject(@"{""value"":117, ""test"":""it works""}"), i);
			}
			return this;
		}

		public Element style(string k, string v){
			if (current.Count == 0) {
				handleStyle (this.gameObject, k, v);
			} else {
				foreach (Element selection in current) {
					handleAttr (selection.gameObject, k, v);
				}
			}
			return this;
		}

		private void handleStyle(GameObject go, string k, string v){
			Text t = go.GetComponent<Text> ();
			Image img = go.GetComponent<Image> ();
			switch (k) {
			case "fill":
				Color c = new Color ();
				ColorUtility.TryParseHtmlString (v, out c);
				if (t != null){
					t.color = c;
				} else if (img != null) {
					img.color = c;
				}
				break;

			default:
				Debug.Log ("attr " + k + " not recognized");
				break;
			}
		}
	}



	public class jsFunction{
		string k;

		public jsFunction(string code){
			Debug.Log("function>code is still a WIP" +code);
			Regex wrapper = new Regex(@"function\((\w+)?,?(\w+)\)\s+?{"); 
			code.Replace('\n',' '); 
			Debug.Log(wrapper.Split(code));
			k = "value";
		}
		public void execute(JsonData d, int i){
			Debug.Log ("execute " + d[k]);
		}
	}

	class Svg{
		XmlDocument contents;
		GameObject gameObject;
		Element root;

		public Svg(){
			this.contents = new XmlDocument ();
			this.gameObject = new GameObject ();
			this.root = new Element (this.gameObject);
		}

		public Svg LoadFromFile(string filePath){
			this.gameObject.name = filePath;
			this.contents.Load (Application.dataPath + filePath);
			return this;
		}

		public Svg LoadFromWeb(string url){
			Debug.Log ("TBD");
			return this;
		}

		public void Render(){
			ProcessElement (this.contents.FirstChild, this.root);
		}

		void ProcessElement(XmlNode node, Element selection){
			selection = selection.append (node.Name);

			if (node.Attributes != null) {
				foreach (XmlAttribute attr in node.Attributes) {

					switch (attr.Name) {
					case "style":
						foreach (string css in attr.Value.Split(';')) {
							string[] kv = css.Split (':');
							Debug.Log (css);
							if (kv.Length == 2) {
								selection.style (kv [0].Trim(' '), kv [1].Trim(' '));			
							}
						}
						break;
					default:
						selection.attr (attr.Name, attr.Value);
						break;
					}
				}
			}

			if (node.Name == "text") {
				selection.text (node.InnerText);
				return;
			}

			foreach (XmlNode child in node.ChildNodes) {
				if (child.Name == "#text") {
					return;
				}
				ProcessElement (child, selection);
			}
		}
	}

}

