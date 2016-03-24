﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using System.IO.Compression;

public class StartingScreen : MonoBehaviour {

	public		MapLuaParser		Scenario;
	public		GetGamedataFile		GamedataFiles;
	public		InputField			Folder;
	public		InputField			Name;
	public		RawImage			Img;
	public		Texture				EmptyMapTexture;
	public		HeaderClass			LoadDDsHeader;

	[System.Serializable]
	public class HeaderClass{
		public		uint size;
		public		uint flags;
		public		uint height;
		public		uint width;
		public		uint sizeorpitch;
		public		uint depth;
		public		uint mipmapcount;
		public		uint alphabitdepth;
		public		uint[] reserved;

		public		uint pixelformatSize;
		public		uint pixelformatflags;
		public		uint pixelformatFourcc;
		public		uint pixelformatRgbBitCount;
		public		uint pixelformatRbitMask;
		public		uint pixelformatGbitMask;
		public		uint pixelformatBbitMask;
		public		uint pixelformatAbitMask;
	}

	void OnEnable(){
		Scenario.FolderName = PlayerPrefs.GetString("LastFolder", "");
		Scenario.ScenarioFileName = PlayerPrefs.GetString("LastScenario", "");

		UpdateFields();
	}

	public void InputEnd(){
		Scenario.FolderName = Folder.text;
		Scenario.ScenarioFileName = Name.text;

		PlayerPrefs.SetString("LastFolder", Folder.text);
		PlayerPrefs.SetString("LastScenario", Name.text);
	}

	public void UpdateFields(){
		Folder.text = Scenario.FolderName;
		Name.text = Scenario.ScenarioFileName;

		PlayerPrefs.SetString("LastFolder", Folder.text);
		PlayerPrefs.SetString("LastScenario", Name.text);

		LoadPreview();
	}

	public void LoadPreview(){
		string MapPath = PlayerPrefs.GetString("MapsPath", "maps/");
		string path = Application.dataPath + "/" + MapPath + Scenario.FolderName;
		#if UNITY_EDITOR
		path = path.Replace("Assets/", "");
		#endif
		byte[] FinalTextureData;
		Vector2	ImageSize = Vector2.one;
		string	FinalImagePath = "";

		if(File.Exists(path + "/preview.jpg")){
			FinalImagePath = path + "/preview.jpg";
			ImageSize *= 256;
		}
		else if(File.Exists(path + "/" + Scenario.FolderName + ".dds")){
			FinalImagePath = path + "/" + Scenario.FolderName + ".dds";
			byte[] FinalTextureData2 = System.IO.File.ReadAllBytes(FinalImagePath);


			byte ddsSizeCheck = FinalTextureData2[4];
			if (ddsSizeCheck != 124)
				throw new Exception("Invalid DDS DXTn texture. Unable to read"); //this header byte should be 124 for DDS image files

			// Load DDS Header
			/*System.IO.FileStream fs = new System.IO.FileStream(FinalImagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			BinaryReader Stream = new BinaryReader(fs);
			LoadDDsHeader = new HeaderClass();

			byte[] signature = Stream.ReadBytes(4);
			LoadDDsHeader.size = Stream.ReadUInt32();
			LoadDDsHeader.flags = Stream.ReadUInt32();
			LoadDDsHeader.height = Stream.ReadUInt32();
			LoadDDsHeader.width = Stream.ReadUInt32();
			LoadDDsHeader.sizeorpitch = Stream.ReadUInt32();
			LoadDDsHeader.depth = Stream.ReadUInt32();
			LoadDDsHeader.mipmapcount = Stream.ReadUInt32();
			LoadDDsHeader.alphabitdepth = Stream.ReadUInt32();


			LoadDDsHeader.reserved = new uint[10];
			for (int i = 0; i < 10; i++)
			{
				LoadDDsHeader.reserved[i] = Stream.ReadUInt32();
			}

			LoadDDsHeader.pixelformatSize = Stream.ReadUInt32();
			LoadDDsHeader.pixelformatflags = Stream.ReadUInt32();
			LoadDDsHeader.pixelformatFourcc = Stream.ReadUInt32();
			LoadDDsHeader.pixelformatRgbBitCount = Stream.ReadUInt32();
			LoadDDsHeader.pixelformatRbitMask = Stream.ReadUInt32();
			LoadDDsHeader.pixelformatGbitMask = Stream.ReadUInt32();
			LoadDDsHeader.pixelformatBbitMask = Stream.ReadUInt32();
			LoadDDsHeader.pixelformatAbitMask = Stream.ReadUInt32();*/


			int height = FinalTextureData2[13] * 256 + FinalTextureData2[12];
			int width = FinalTextureData2[17] * 256 + FinalTextureData2[16];

			TextureFormat format = GamedataFiles.GetFormatOfDds(FinalImagePath);


			Texture2D textureDds = new Texture2D(width, height, format, false);
			int DDS_HEADER_SIZE = 128;
			byte[] dxtBytes = new byte[FinalTextureData2.Length - DDS_HEADER_SIZE];
			Buffer.BlockCopy(FinalTextureData2, DDS_HEADER_SIZE, dxtBytes, 0, FinalTextureData2.Length - DDS_HEADER_SIZE);
			textureDds.LoadRawTextureData(dxtBytes);
			textureDds.Apply();

			Img.texture = textureDds;
			return;
		}
		else if(File.Exists(path + "/" + Scenario.FolderName + ".png")){
			FinalImagePath = path + "/" + Scenario.FolderName + ".png";
			ImageSize *= 256;
		}
		else if(File.Exists(path + "/" + Scenario.FolderName + ".small" + ".png")){
			FinalImagePath = path + "/" + Scenario.FolderName + ".small" + ".png";
			ImageSize *= 100;
		}
		else{
			// No image
			Debug.LogWarning("no image");
			Img.texture = EmptyMapTexture;
			return;
		}
		Debug.Log(FinalImagePath);

		FinalTextureData = System.IO.File.ReadAllBytes(FinalImagePath);
		Texture2D texture = new Texture2D((int)ImageSize.x, (int)ImageSize.y);
		texture.LoadImage(FinalTextureData);
		
		Img.texture = texture;
	}

}