using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Ronners.Bot.Models
{
    public class Canvas
    {
        public Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image {get;set;}
        public Color penColor {get;set;}

        public bool fill{get;set;}

        public void SetPenColor(ushort r, ushort g, ushort b, ushort a)
        {
            penColor = new Color(new Rgba32(r,g,b,a));
        }
        public void SetPenColor(Color color)
        {
            penColor = color;
        }

        public void Draw(int x, int y)
        {
            if(x>= image.Width || x < 0 || y>= image.Height || y < 0 )
                return;
            image[x,y] = penColor;
        }

        public Canvas(int width, int height, Color background)
        {
            image = new Image<Rgba32>(width,height,background);
            fill = false;
        }

        public void Save(string path)
        {
            image.SaveAsPng(path);
        }

        public void DrawLine(int x0, int x1, int y0, int y1)
        {
            int dx = Math.Abs(x1-x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1-y0);
            int sy = y0 < y1 ? 1 : -1;
            var err = dx+dy;
            while(true)
            {
                Draw(x0,y0);
                if(x0==x1 && y0==y1)
                    break;
                var err2 = 2*err;
                if (err2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }
                if(err2 <= dx)
                {
                    err+=dx;
                    y0+=sy;
                }
            }
        }

        public void DrawRectangle(int x, int y, int width, int height)
        {
            if(fill)
            {
                for(int i = y;i<=y+height-1;i++)
                {
                    DrawLine(x,x+width-1,i,i);
                }
            }
            else
            {
                DrawLine(x,x,y,y+height-1);
                DrawLine(x,x+width-1,y+height-1,y+height-1);
                DrawLine(x+width-1,x+width-1,y+height-1,y);
                DrawLine(x+width-1,x,y,y);
            }
            
        }


        public void DrawCirclePoints(int x, int y , int dx, int dy)
        {
            if(fill)
            {
                DrawLine(x-dx,x+dx,y+dy,y+dy);
                DrawLine(x-dx,x+dx,y-dy,y-dy);
                DrawLine(x-dy,x+dy,y-dx,y-dx);
                DrawLine(x-dy,x+dy,y+dx,y+dx);
            }
            else
            {
                Draw(x+dx,y+dy);
                Draw(x-dx,y+dy);
                Draw(x+dx,y-dy);
                Draw(x-dx,y-dy);
                Draw(x+dy,y+dx);
                Draw(x-dy,y+dx);
                Draw(x+dy,y-dx);
                Draw(x-dy,y-dx);
            }
        }

        public void DrawCircle(int x, int y, int radius)
        {
            int dx = 0;
            int dy = radius;
            int d = (5 - radius * 4)/4;
            DrawCirclePoints(x,y,dx,dy);
            while(dy>=dx)
            {
                dx++;
                if(d > 0)
                {
                    d += 2 * (dx-dy) + 1;
                    dy--;
                }
                else
                    d += 2 * dx + 1;
                DrawCirclePoints(x,y,dx,dy);
            }
        }
    }


    public static class Colors
    {
        public static SixLabors.ImageSharp.PixelFormats.Rgba32 Red 
        {
            get
            {
                return new SixLabors.ImageSharp.PixelFormats.Rgba32(255,0,0,0);
            }
        }
        public static SixLabors.ImageSharp.PixelFormats.Rgba32 Blue 
        {
            get
            {
                return new SixLabors.ImageSharp.PixelFormats.Rgba32(0,0,255,1);
            }
        }
        public static SixLabors.ImageSharp.PixelFormats.Rgba32 Green     
        {
            get
            {
                return new SixLabors.ImageSharp.PixelFormats.Rgba32(0,255,0,1);
            }
        }
        public static SixLabors.ImageSharp.PixelFormats.Rgba32 Black     
        {
            get
            {
                return new SixLabors.ImageSharp.PixelFormats.Rgba32(255,255,255,1);
            }
        }
        public static SixLabors.ImageSharp.PixelFormats.Rgba32 White     
        {
            get
            {
                return new SixLabors.ImageSharp.PixelFormats.Rgba32(0,0,0,0);
            }
        }
    }
}
