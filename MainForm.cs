using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SitesMonitor.Containers;

namespace SitesMonitor
{
	public partial class MainForm : Form
	{
		int x, y;
    	bool s = false;
    	string data_path = "sites.json";
    	SitesContainer sites_container;
    	
		public MainForm()
		{
			InitializeComponent();
			pictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dragForm);
	        pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dragForm);
			pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
			
			string json = "";
			if(File.Exists(data_path)) {
				json = File.ReadAllText(data_path);
				sites_container = JsonConvert.DeserializeObject<SitesContainer>(json);
			} else {
				sites_container = new SitesContainer();
				sites_container.sites = new SiteData[] {
					new SiteData(), new SiteData()
				};
				json = JsonConvert.SerializeObject(sites_container);
				File.WriteAllText(data_path, json);
			}
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
		}
		
		private void updateForm() {
			this.Refresh();
		}
		
		private void dragForm(object sender, MouseEventArgs e) {
    		//перетаскивание формы
	        if (s == false) { x = e.X; y = e.Y; s = true; }
	        if (e.Button.ToString() == "Left") {
	        	this.Location = new Point(this.Left + e.X - x, this.Top + e.Y - y);
	        } else { s = false; }
	    }
		
		public static bool testSite(string url) {
			Uri uri = new Uri(url);
			return testSite(uri);
		}
		
		public static bool testSite(Uri uri) {
			HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(uri);
			try {
				HttpWebResponse response = (HttpWebResponse) request.GetResponse();
				//Debug.Print(uri.Host + " available");
				
			} catch(WebException exception) {
				//Debug.Print(uri.Host + " not available");
				//string response = (HttpWebResponse)exception.Response;
				string response = exception.Message;
				Debug.Print(response);
				return false;
			}
			return true;
		}
		
		void refreshSites() {
			Bitmap bmp = new Bitmap(pictureBox.Image);
			int container_width  = pictureBox.Width;
			int container_height = pictureBox.Height;
			int rows_count = (int) Math.Ceiling(Math.Sqrt((double)sites_container.sites.Length));
			//int columns_count = 
			int rows_width = (int) Math.Floor((double)container_width / (double)rows_count);
			Color online_color = Color.FromArgb(255,118,255,3);
			Color offline_color = Color.FromArgb(255,221,44,0);
			Color background_color = Color.FromArgb(255,105,105,105);
			Color frame_color = Color.FromArgb(255,128,128,128);
			Color text_color = Color.FromArgb(255,32,32,32);
			
			Graphics gr = Graphics.FromImage(bmp);
			Brush fill_brush = new SolidBrush(background_color);
			Brush online_brush = new SolidBrush(online_color);
			Brush offline_brush = new SolidBrush(offline_color);
			Brush text_brush = new SolidBrush(text_color);
			
			Pen border_pen = new Pen(frame_color);
			gr.FillRectangle(fill_brush, 0, 0, container_width, container_height);
			updateForm();
			Font draw_font = new Font("Times New Roman", 20, FontStyle.Regular, GraphicsUnit.Pixel);
			
			int block_index = 0;
			for(int x=0; x < rows_count; x++) {
				for(int y=0; y < rows_count; y++) {
					if(block_index < sites_container.sites.Length) {
						Rectangle rect = new Rectangle(x * rows_width, y * rows_width, rows_width, rows_width);
						Uri site_uri = new Uri(sites_container.sites[block_index].url);
						bool result = testSite(site_uri);
						Debug.Print(result.ToString());
						if(result) {
							gr.FillRectangle(online_brush, rect);
						} else {
							gr.FillRectangle(offline_brush, rect);
						}
						//рамка
						gr.DrawRectangle(border_pen, rect);
						//текст
						//string site_host = site_uri.Host;
						string site_host = sites_container.sites[block_index].name;
						SizeF text_size = gr.MeasureString(site_host, draw_font);
						float text_x = rows_width * (x + 0.5f) - text_size.Width/2;
						float text_y = rows_width * (y + 0.5f) - text_size.Height/2;
						gr.DrawString(site_host, draw_font, text_brush, text_x, text_y);
						block_index++;
					} else {
						break;
					}
					pictureBox.Image = bmp;
					updateForm();
				}
			}
			pictureBox.Image = bmp;
			updateForm();
		}
		
		void Timer_updateSitesTick(object sender, EventArgs e)
		{
			refreshSites();
		}
		
		void MainFormShown(object sender, EventArgs e)
		{
			refreshSites();
			timer_updateSites.Enabled = true;
			timer_updateSites.Start();
		}
	}
}
