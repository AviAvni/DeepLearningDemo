module Visualizer

open System.Drawing

let tile = 20

let draw (pixels:float[]) (text:string) ok =

    let bitmap = new Bitmap(29 * tile, 29 * tile)

    let graphics = Graphics.FromImage(bitmap)
                
    pixels 
    |> Array.iteri (fun i p ->
        let col = i % 28
        let row = i / 28
        let color = Color.FromArgb(int p, int p, int p)
        let brush = new SolidBrush(color)
        graphics.FillRectangle(brush,col*tile,row*tile,tile,tile))

    let point = new PointF((float32)5, (float32)5)
    let font = new Font(family = FontFamily.GenericSansSerif, emSize = (float32)12)        
    let textColor =
        if ok then new SolidBrush(Color.Green)
        else new SolidBrush(Color.Red)
    graphics.DrawString(text, font, textColor, point)
        
    bitmap
