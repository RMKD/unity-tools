# unity-tools

This is a repository to keep track of some cross-language experiments I'm doing in Unity. 

## python

Now that IronPython is under development again, I wanted to see if I could combine it with Unity. There were some older community questions about running Python scripts but it was challenging to get them working with Unity 5.5. What can you do with it? I'm not sure yet, but this setup with an older version of IronPython will open a python REPL in the Editor and demonstrates how to run stored python scripts. 

## D3 

If you're doing data visualizations in immersive VR environments, this is a tool that makes it easy(-ish) to render d3 visualizations on a Canvas in worldspace using standard Unity UI elements. It's a bit of a hack but the basic idea is to set up a class that allows you to use d3 javascript syntax and conventions within a C# script. Weird, eh? But it kind of works. The biggest challenge, of course, will be handling the data binding. In the meantime it is a quick way to handle some of the challenging parts of interacting with the UI programmatically and does reasonably well at parsing SVG exports (so far, just testing on vega-lite).

```
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using D3Sharp;

public class D3SharpTesting : MonoBehaviour {

	void Start () {
		var d3 = new Element (this.gameObject);
		var svg = d3.append ("svg")
			.attr ("width",800)
			.attr ("height", 600);

		svg.append ("g").attr ("id", "test-group")
			.append ("rect")
			    .attr("x",150)
			    .attr("y",10)
			    .attr("width",60)
			    .attr("height",100)
			    .attr("fill","#337700");
```
