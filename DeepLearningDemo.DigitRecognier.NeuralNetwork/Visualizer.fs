module Visualizer

open System.Drawing

let tile = 20

let draw (digit:int*float[]) (text:string) =

    let bitmap = new Bitmap(29 * tile, 29 * tile)

    let graphics = Graphics.FromImage(bitmap)
        
    let n, pixels = digit
        
    pixels 
    |> Array.iteri (fun i p ->
        let col = i % 28
        let row = i / 28
        let color = Color.FromArgb(int p, int p, int p)
        let brush = new SolidBrush(color)
        graphics.FillRectangle(brush,col*tile,row*tile,tile,tile))

    let point = new PointF((float32)5, (float32)5)
    let font = new Font(family = FontFamily.GenericSansSerif, emSize = (float32)12)        
    graphics.DrawString(text, font, new SolidBrush(Color.Red), point)
        
    bitmap
