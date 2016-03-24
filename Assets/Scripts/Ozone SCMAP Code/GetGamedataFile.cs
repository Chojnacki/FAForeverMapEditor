﻿using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.BZip2;


#pragma warning disable 0162

public class GetGamedataFile : MonoBehaviour {
	const bool DebugTextureLoad = false;

	public static	string			GameDataPath;
	public			ScmapEditor		Scmap;
	public			Texture2D		EmptyTexture;

	public static float MipmapBias = 0.5f;
	public static int AnisoLevel = 10;

	public void SetPath(){
		GameDataPath = PlayerPrefs.GetString("GameDataPath", "gamedata/");
	}

	public void LoadTextureFromGamedata(string scd, string LocalPath, int Id, bool NormalMap = false){
		if(string.IsNullOrEmpty(LocalPath)) return;
		SetPath();

		if(!Directory.Exists(GameDataPath)){
			Debug.LogError("Gamedata path not exist!");
			return;
		}

		ZipFile zf = null;
		try {
			FileStream fs = File.OpenRead(GameDataPath + scd);
			zf = new ZipFile(fs);

			//char[] sep = ("/").ToCharArray();
			//string[] LocalSepPath = LocalPath.Split(sep);
			//string FileName = LocalSepPath[LocalSepPath.Length - 1];

			ZipEntry zipEntry2 =  zf.GetEntry(LocalPath);
			if(zipEntry2 == null){
				Debug.LogError("Zip Entry is empty for: " + LocalPath);
				return;
			}

			byte[] FinalTextureData2 = new byte[4096]; // 4K is optimum

			if (zipEntry2 != null)
			{
				Stream s = zf.GetInputStream(zipEntry2);
				FinalTextureData2 = new byte[zipEntry2.Size];
				s.Read(FinalTextureData2, 0, FinalTextureData2.Length);
			}

			TextureFormat format = GetFormatOfDdsBytes(FinalTextureData2);
			bool Mipmaps = LoadDDsHeader.mipmapcount > 0;
			Texture2D texture = new Texture2D((int)LoadDDsHeader.width, (int)LoadDDsHeader.height, format, Mipmaps, true);

			int DDS_HEADER_SIZE = 128;
			byte[] dxtBytes = new byte[FinalTextureData2.Length - DDS_HEADER_SIZE];
			Buffer.BlockCopy(FinalTextureData2, DDS_HEADER_SIZE, dxtBytes, 0, FinalTextureData2.Length - DDS_HEADER_SIZE);
			texture.LoadRawTextureData(dxtBytes);
			texture.Apply(false);

			if(NormalMap){
				texture.Compress(true);

				Texture2D normalTexture = new Texture2D((int)LoadDDsHeader.width, (int)LoadDDsHeader.height, TextureFormat.RGBA32, Mipmaps, true);

				Color theColour = new Color();
				Color[] Pixels;

				for(int m = 0; m < LoadDDsHeader.mipmapcount + 1; m++){
					int Texwidth = texture.width;
					int Texheight = texture.height;

					if(m > 0){
						Texwidth /= (int)Mathf.Pow(2, m);
						Texheight /= (int)Mathf.Pow(2, m);
					}
					Pixels = texture.GetPixels(0, 0, Texwidth, Texheight, m);

					for(int i = 0; i < Pixels.Length; i++){
						theColour.r = Pixels[i].r;
						theColour.g = Pixels[i].g;
						theColour.b = 1;
						theColour.a = Pixels[i].g;
						Pixels[i] = theColour;
					}
					normalTexture.SetPixels(0, 0, Texwidth, Texheight, Pixels, m);
				}

				normalTexture.Apply(false);

				Scmap.Textures[Id].Normal = normalTexture;
				Scmap.Textures[Id].Normal.mipMapBias = MipmapBias;
				Scmap.Textures[Id].Normal.filterMode = FilterMode.Bilinear;
				Scmap.Textures[Id].Normal.anisoLevel = AnisoLevel;
			}
			else{
				Scmap.Textures[Id].Albedo = texture;
				Scmap.Textures[Id].Albedo.mipMapBias = MipmapBias;
				Scmap.Textures[Id].Albedo.filterMode = FilterMode.Bilinear;
				Scmap.Textures[Id].Albedo.anisoLevel = AnisoLevel;
			}

			/*
			foreach (ZipEntry zipEntry in zf) {
				break;

				if (!zipEntry.IsFile) {
					continue;
				}
				if(zipEntry.Name.ToLower() == LocalPath.ToLower() || zipEntry.Name == LocalPath.ToLower()){
					byte[] buffer = new byte[4096]; // 4K is optimum
					Stream zipStream = zf.GetInputStream(zipEntry);
					int size = 4096;
		
					if(!File.Exists("temfiles/" + FileName)){
						using (FileStream streamWriter = File.Create("temfiles/" + FileName))
						{
							while (true)
								{
								size = zipStream.Read(buffer, 0, buffer.Length);
								if (size > 0)
								{
									streamWriter.Write(buffer, 0, size);
								}
								else
								{
									break;
								}
							}
							streamWriter.Close();
						}
					}


					byte[] FinalTextureData = System.IO.File.ReadAllBytes("temfiles/" + FileName);

					int height = FinalTextureData[13] * 256 + FinalTextureData[12];
					int width = FinalTextureData[17] * 256 + FinalTextureData[16];

					TextureFormat format = GetFormatOfDds("temfiles/" + FileName);
					bool Mipmaps = LoadDDsHeader.mipmapcount > 0;

					Texture2D texture = new Texture2D(width, height, format, Mipmaps, true);

					int DDS_HEADER_SIZE = 128;
					byte[] dxtBytes = new byte[FinalTextureData.Length - DDS_HEADER_SIZE];
					Buffer.BlockCopy(FinalTextureData, DDS_HEADER_SIZE, dxtBytes, 0, FinalTextureData.Length - DDS_HEADER_SIZE);
					texture.LoadRawTextureData(dxtBytes);
					texture.Apply(false);

					if(DebugTextureLoad) Debug.Log(NormalMap + ", " + FileName + ", " + format);

					if(NormalMap){
						texture.Compress(true);

						Texture2D normalTexture = new Texture2D(height, width, TextureFormat.RGBA32, Mipmaps, true);

						Color theColour = new Color();
						Color[] Pixels;

						for(int m = 0; m < LoadDDsHeader.mipmapcount + 1; m++){
							int Texwidth = texture.width;
							int Texheight = texture.height;

							if(m > 0){
								Texwidth /= (int)Mathf.Pow(2, m);
								Texheight /= (int)Mathf.Pow(2, m);
							}
							Pixels = texture.GetPixels(0, 0, Texwidth, Texheight, m);

							for(int i = 0; i < Pixels.Length; i++){
								theColour.r = Pixels[i].r;
								theColour.g = Pixels[i].g;
								theColour.b = 1;
								theColour.a = Pixels[i].g;
								Pixels[i] = theColour;
							}
							normalTexture.SetPixels(0, 0, Texwidth, Texheight, Pixels, m);
						}

						normalTexture.Apply(false);

						Scmap.Textures[Id].Normal = normalTexture;
						Scmap.Textures[Id].Normal.mipMapBias = MipmapBias;
						Scmap.Textures[Id].Normal.filterMode = FilterMode.Bilinear;
						Scmap.Textures[Id].Normal.anisoLevel = AnisoLevel;
					}
					else{
						Scmap.Textures[Id].Albedo = texture;
						Scmap.Textures[Id].Albedo.mipMapBias = MipmapBias;
						Scmap.Textures[Id].Albedo.filterMode = FilterMode.Bilinear;
						Scmap.Textures[Id].Albedo.anisoLevel = AnisoLevel;
					}
				}

			}*/
		} finally {
			if (zf != null) {
				zf.IsStreamOwner = true; // Makes close also shut the underlying stream
				zf.Close(); // Ensure we release resources
			}
		}

	}


	//************************************* SIMPLE LOAD
	public Texture2D LoadSimpleTextureFromGamedata(string scd, string LocalPath, bool NormalMap = false){
		SetPath();
		
		if(!Directory.Exists(GameDataPath)){
			Debug.LogError("Gamedata path not exist!");
			return null;
		}
		
		if(!Directory.Exists("temfiles")) Directory.CreateDirectory("temfiles");
		Texture2D texture = null;

		if(DebugTextureLoad) Debug.LogWarning("Load texture: " + GameDataPath + scd + LocalPath);
		ZipFile zf = null;
		try {
			FileStream fs = File.OpenRead(GameDataPath + scd);
			zf = new ZipFile(fs);

			
			char[] sep = ("/").ToCharArray();
			string[] LocalSepPath = LocalPath.Split(sep);
			string FileName = LocalSepPath[LocalSepPath.Length - 1];
			
			
			foreach (ZipEntry zipEntry in zf) {
				if (!zipEntry.IsFile) {
					continue;
				}
				if(DebugTextureLoad) Debug.Log(zipEntry.Name.ToLower() + " - " + LocalPath.ToLower());

				if(zipEntry.Name.ToLower() == LocalPath.ToLower() || zipEntry.Name == LocalPath.ToLower() || ("/" + zipEntry.Name).ToLower() == LocalPath.ToLower()){
					if(DebugTextureLoad) Debug.LogWarning("File found!");
					
					byte[] buffer = new byte[4096]; // 4K is optimum
					Stream zipStream = zf.GetInputStream(zipEntry);
					int size = 4096;
					//File.Create("temfiles/" + FileName).Dispose();
					using (FileStream streamWriter = File.Create("temfiles/" + FileName))
					{
						while (true)
						{
							size = zipStream.Read(buffer, 0, buffer.Length);
							if (size > 0)
							{
								streamWriter.Write(buffer, 0, size);
							}
							else
							{
								break;
							}
						}
						streamWriter.Close();
					}
					
					
					
					byte[] FinalTextureData = System.IO.File.ReadAllBytes("temfiles/" + FileName);
					
					byte ddsSizeCheck = FinalTextureData[4];
					if (ddsSizeCheck != 124)
						throw new Exception("Invalid DDS DXTn texture. Unable to read"); //this header byte should be 124 for DDS image files
					
					int height = FinalTextureData[13] * 256 + FinalTextureData[12];
					int width = FinalTextureData[17] * 256 + FinalTextureData[16];
					
					TextureFormat format;

					format = GetFormatOfDds("temfiles/" + FileName);
					
					texture = new Texture2D(width, height, format, true, false);
					int DDS_HEADER_SIZE = 128;
					byte[] dxtBytes = new byte[FinalTextureData.Length - DDS_HEADER_SIZE];
					Buffer.BlockCopy(FinalTextureData, DDS_HEADER_SIZE, dxtBytes, 0, FinalTextureData.Length - DDS_HEADER_SIZE);
					texture.LoadRawTextureData(dxtBytes);
					//texture.Apply(false);
					
					if(NormalMap){
						Texture2D normalTexture = new Texture2D(height, width, TextureFormat.ARGB32, true);
						
						Color theColour = new Color();
						Color[] Pixels;

						for(int m = 0; m < LoadDDsHeader.mipmapcount; m++){
							Pixels = normalTexture.GetPixels(m);

							for(int i = 0; i < Pixels.Length; i++){
								theColour.r = Pixels[i].r;
								theColour.g = Pixels[i].g;
								theColour.b = 1;
								theColour.a = Pixels[i].g;
								Pixels[i] = theColour;
							}
							normalTexture.SetPixels(Pixels, m);
						}

						normalTexture.Apply(false);

						normalTexture.mipMapBias = MipmapBias;
						normalTexture.anisoLevel = AnisoLevel;
						normalTexture.filterMode = FilterMode.Bilinear;

						texture = normalTexture;
					}
					else{
						texture.mipMapBias = MipmapBias;
						texture.filterMode = FilterMode.Bilinear;
						texture.anisoLevel = AnisoLevel;

					}
					
				}
			}
		} finally {
			if (zf != null) {
				zf.IsStreamOwner = true; // Makes close also shut the underlying stream
				zf.Close(); // Ensure we release resources
			}
		}

		Debug.LogError("Gamedata path not exist!");
		return texture;
	}

	public		HeaderClass			LoadDDsHeader;
	
	[System.Serializable]
	public class HeaderClass{
		public		TextureFormat		Format;
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
		public		int 		DebugSize;
	}


	public TextureFormat GetFormatOfDdsBytes(byte[] bytes){

		// Load DDS Header

		//System.IO.FileStream fs = new System.IO.FileStream(FinalImagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
		Stream ms = new MemoryStream(bytes);
		BinaryReader Stream = new BinaryReader(ms);
		LoadDDsHeader = new HeaderClass();

		Stream.ReadBytes(4);
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
		LoadDDsHeader.pixelformatAbitMask = Stream.ReadUInt32();

		return ReadFourcc(LoadDDsHeader.pixelformatFourcc);
	}

	public TextureFormat GetFormatOfDds(string FinalImagePath){

		if(!File.Exists(FinalImagePath)){
			Debug.LogError("File not exist!");
			return TextureFormat.DXT5;
		}

		// Load DDS Header

		System.IO.FileStream fs = new System.IO.FileStream(FinalImagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
		BinaryReader Stream = new BinaryReader(fs);
		LoadDDsHeader = new HeaderClass();
		
		Stream.ReadBytes(4);
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
		LoadDDsHeader.pixelformatAbitMask = Stream.ReadUInt32();

		return ReadFourcc(LoadDDsHeader.pixelformatFourcc);
	}

	public static HeaderClass GetDdsFormat(byte[] Bytes){
		Stream fs = new MemoryStream(Bytes);
		BinaryReader Stream = new BinaryReader(fs);
		HeaderClass DDsHeader = new HeaderClass();

		Stream.ReadBytes(4);
		DDsHeader.size = Stream.ReadUInt32();
		DDsHeader.flags = Stream.ReadUInt32();
		DDsHeader.height = Stream.ReadUInt32();
		DDsHeader.width = Stream.ReadUInt32();
		DDsHeader.sizeorpitch = Stream.ReadUInt32();
		DDsHeader.depth = Stream.ReadUInt32();
		DDsHeader.mipmapcount = Stream.ReadUInt32();
		DDsHeader.alphabitdepth = Stream.ReadUInt32();


		DDsHeader.reserved = new uint[10];
		for (int i = 0; i < 10; i++)
		{
			DDsHeader.reserved[i] = Stream.ReadUInt32();
		}

		DDsHeader.pixelformatSize = Stream.ReadUInt32();
		DDsHeader.pixelformatflags = Stream.ReadUInt32();
		DDsHeader.pixelformatFourcc = Stream.ReadUInt32();
		DDsHeader.pixelformatRgbBitCount = Stream.ReadUInt32();
		DDsHeader.pixelformatRbitMask = Stream.ReadUInt32();
		DDsHeader.pixelformatGbitMask = Stream.ReadUInt32();
		DDsHeader.pixelformatBbitMask = Stream.ReadUInt32();
		DDsHeader.pixelformatAbitMask = Stream.ReadUInt32();

		return DDsHeader;
	}


	public TextureFormat ReadFourcc(uint fourcc){

		if(DebugTextureLoad) Debug.Log(
			"Size: " + LoadDDsHeader.size +
			" flags: " + LoadDDsHeader.flags +
			" height: " + LoadDDsHeader.height +
			" width: " + LoadDDsHeader.width +
			" sizeorpitch: " + LoadDDsHeader.sizeorpitch +
			" depth: " + LoadDDsHeader.depth +
			" mipmapcount: " + LoadDDsHeader.mipmapcount +
			" alphabitdepth: " + LoadDDsHeader.alphabitdepth +
			" pixelformatSize: " + LoadDDsHeader.pixelformatSize +
			" pixelformatflags: " + LoadDDsHeader.pixelformatflags +
			" pixelformatFourcc: " + LoadDDsHeader.pixelformatFourcc +
			" pixelformatRgbBitCount: " + LoadDDsHeader.pixelformatRgbBitCount +
			" pixelformatRbitMask: " + LoadDDsHeader.pixelformatRbitMask +
			" pixelformatGbitMask: " + LoadDDsHeader.pixelformatGbitMask +
			" pixelformatBbitMask: " + LoadDDsHeader.pixelformatBbitMask +
			" pixelformatAbitMask: " + LoadDDsHeader.pixelformatAbitMask
		);


		switch(fourcc){
		case 827611204:
			return TextureFormat.DXT1;
		case 894720068:
			return TextureFormat.DXT5;
		case 64:
			return TextureFormat.RGB24;
		case 0:
			if(LoadDDsHeader.flags == 528391){
				return TextureFormat.BGRA32;
			}
			else if(LoadDDsHeader.pixelformatRgbBitCount == 24){
				return TextureFormat.RGB24;
			}
			else{
				return TextureFormat.BGRA32;
			}
		}
		
		return TextureFormat.DXT5;
	}
}