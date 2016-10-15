using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
		JsonData __data__;
		Sprite circle = Resources.Load<Sprite> ("white-circle-md");
		Font defaultFont = Resources.GetBuiltinResource (typeof(Font), "Arial.ttf") as Font;

		private void UseHTMLStyleCoordinates(GameObject go, bool centered=false){
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
			this.gameObject.name = "D3Sharp-";
			this.__data__ = JsonMapper.ToObject (@"{}");
		}

		public Element select(string sel){
			//section adds elements to the current list
			current = new List<Element>();
			current.Add(new Element(this.gameObject));//TODO figure out how to deal
			return this;
		}

		public Element selectAll(string sel){
			//section adds elements to the current list
			current = new List<Element>();
			current.Add(new Element(this.gameObject));//TODO figure out how to deal
			return this;
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
				if (element == "svg") {
					this.gameObject.name = "D3Sharp-canvas";
					Canvas c = this.gameObject.AddComponent<Canvas> () as Canvas;
					c.renderMode = RenderMode.WorldSpace;
					this.gameObject.AddComponent<CanvasScaler> ();
					this.gameObject.AddComponent<GraphicRaycaster> ();
					UseHTMLStyleCoordinates (this.gameObject);
				} else {
					handleAppend (this.gameObject, element);
				}
			} else {
				foreach (Element selection in current) {
					handleAppend (selection.gameObject, element);
				}
			}
			return this;
		}

		private void handleAppend(GameObject go, string element){
			if (element == "text") {
				go.name += element;
				Text t = go.AddComponent<Text> ();
				UseHTMLStyleCoordinates (go);
				t.font = defaultFont;
				t.fontStyle = FontStyle.Bold;
				t.color = Color.black;
			} else if (element == "rect") {
				go.name += element;
				Image img = go.AddComponent<Image> ();
				UseHTMLStyleCoordinates (go);
			} else if (element == "circle") {
				go.name += element;
				Image img = go.AddComponent<Image> ();
				UseHTMLStyleCoordinates (go, true);
				img.sprite = circle;
			} else if (element == "path") {Debug.Log (element + " elements are not yet supported");
			} else if (element == "div") {Debug.Log (element + " elements are not yet supported");
			} else if (element == "button") {Debug.Log (element + " elements are not yet supported")
			} else {
				Debug.Log (element + " elements are not yet supported");
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
				//GameObject newObject = new GameObject ();
				//handleAppend (newObject, "text");
				//handleText (newObject, t);
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
			RectTransform t = go.GetComponent<RectTransform> ();

			if (k == "x" || k == "cx") {
				t.anchoredPosition = new Vector2(v, t.anchoredPosition.y);
			} else if (k == "y" || k == "cy") {
				t.anchoredPosition = new Vector2 (t.anchoredPosition.x, -v);
			} else if (k == "width") {
				t.sizeDelta = new Vector2 (v, t.sizeDelta.y);
			} else if (k == "height") {
				t.sizeDelta = new Vector2 (t.sizeDelta.x, v);
			} else {
				Debug.Log ("attr " + k + " not recognized");
			}
		}

		//handle string attributes
		private void handleAttr(GameObject go, string k, string v){
			Image img = go.GetComponent<Image> ();
			if (k == "fill" && v[0] == '#') {
				if (!img) {
					go.AddComponent<Image> ();
				}
				Color c = new Color ();
				ColorUtility.TryParseHtmlString (v, out c);
				img.color = c;
			} else if (k == "id") {
				go.name = "#" + v;
			} else if (k == "class") {
				go.tag = v.ToString ();
			} else {
				Debug.Log ("attr " + k + " not recognized");
			}
		}
        
			
		public Element attr(string k, jsFunction f){
			Debug.Log ("functions are not yet supported for " + k);
			for(var i=0; i < current.Count; i++){
				f.execute (JsonMapper.ToObject(@"{""value"":117, ""test"":""it works""}"), i);
			}
			return this;
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

}

