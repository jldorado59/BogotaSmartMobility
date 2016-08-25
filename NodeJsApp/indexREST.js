function getBounds(source)
{
	var bounds = { xmin:1000, xmax:-1000, zmin:1000, zmax:-1000 }	
	
	for(i=0; i < source.features.length; i++)
	{
		var geometry = source.features[i].geometry;
		var coords = geometry.coordinates[0];
		for(j=0; j < coords.length; j++)
		{
			var point = coords[j];
			var x = point[0];
			var z = point[1];
			
			if(x < bounds.xmin)
				bounds.xmin = x;
			if(x > bounds.xmax)
				bounds.xmax = x;
			if(z < bounds.zmin)
				bounds.zmin = z;
			if(z > bounds.zmax)		
				bounds.zmax = z;
		}
	}
	
	return bounds;
}

function getCentroid(points)
{	
	var centroid = {x:0,z:0};	
	var x0, z0, x1, z1, area, signedarea=0;        

	// For all vertices except last
	for (var i = 0; i < points.length - 1; i++)
	{		
		x0 = points[i].x;
		z0 = points[i].z;
		x1 = points[i + 1].x;
		z1 = points[i + 1].z;
		area = x0 * z1 - x1 * z0;
		signedarea += area;
		centroid.x += (x0 + x1) * area;
		centroid.z += (z0 + z1) * area;
	}
	
	signedarea = signedarea / 2;
	centroid.x = centroid.x / (6 * signedarea);
	centroid.z = centroid.z / (6 * signedarea);        
	
	return centroid;
}

function getDataNormalized(source, bounds)
{
	var features = [];
	
	for(i=0; i < source.features.length; i++)
	{
		var objectid = source.features[i].properties.objectid;
		var numpisos = source.features[i].properties.connpisos;
		var coords = source.features[i].geometry.coordinates[0];		
		
		var geometry = { id:objectid, pisos:numpisos, centroid:null, points:[]}
		
		for(j=0; j < coords.length; j++)
		{			
			var x = coords[j][0];
			var z = coords[j][1];
			
			x = (bounds.xmax - x)/(bounds.xmax - bounds.xmin); 		
			z = (bounds.zmax - z)/(bounds.zmax - bounds.zmin); 
			
			var point = { x:x, z:z };			
			geometry.points.push(point);
		}
		
		geometry.centroid = getCentroid(geometry.points);
		features.push(geometry);		
	}
	
	return features;
}

function getFeaturesByRadio(features, position, radio)
{
	var zonefeatures = []; 
	for(i=0; i < features.length; i++)
	{
		
		if(	features[i].centroid.x > position.x - radio && features[i].centroid.x < position.x + radio && 
			features[i].centroid.z > position.z - radio && features[i].centroid.z < position.z + radio )
		{
			zonefeatures.push(features[i]);
		}		
	}	
	
	return zonefeatures;
}

function getFullJson(features)
{
	var fulljson = "{ \"features\":["; 
	
	for(i=0; i < features.length; i++)
	{
		var points = features[i].points;
		
		fulljson += "{";
		fulljson += "\"id\":" + features[i].id + ",";	
		fulljson += "\"pisos\":" + features[i].pisos + ",";	
		fulljson += "\"centroid\":{\"x\":" + features[i].centroid.x + ", \"z\":" + features[i].centroid.z + "},"
		fulljson += "\"points\":["

		for(j=0; j < points.length; j++)
		{		
			fulljson += "{ \"x\":" + points[j].x + ", \"z\":" + points[j].z + "},"					
		}
		fulljson = fulljson.substring(0, fulljson.length - 1);	
		fulljson += "] },"			
	}		
	
	fulljson = fulljson.substring(0, fulljson.length - 1);
	fulljson += "] }"

	fs = require('fs');
	fs.writeFile("fulldata.json", fulljson);
	
	return fulljson;
}


///--------------- main -----------------

var express = require('express')
 
var app = express()

var fs = require('fs'); 
var contents = fs.readFileSync('data.json', "utf-8"); 
var source = JSON.parse(contents);
var bounds = getBounds(source);
var data = getDataNormalized(source, bounds);
//var fulljson = getFullJson(data); 

console.log("ready!");

app.get('/data', function(req, res) {    

	console.log("x:"+req.query.x + ",z:" + req.query.z + ",radio:" + req.query.radio);
	var pos = { x: req.query.x, z: req.query.z };	
	var features = getFeaturesByRadio(data, pos, req.query.radio);
	var json = getFullJson(features);
	console.log("response");
	res.status(200).json(json);
   
})
 
app.listen(3000)
