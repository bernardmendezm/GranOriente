# Versión 8 - Corrección de factura detallada

## Problema corregido

En la versión anterior algunas expresiones Razor estaban partidas en varias líneas.

Ejemplo anterior:

```cshtml
@Model.FechaEmision
    .ToString("dd/MM/yyyy HH:mm")
```

Razor interpretaba únicamente:

`@Model.FechaEmision`

como código C# y mostraba:

`.ToString("dd/MM/yyyy HH:mm")`

como texto visible.

## Solución

Ahora se utiliza:

```cshtml
@(Model.FechaEmision.ToString("dd/MM/yyyy HH:mm"))
```

El mismo cambio se aplicó en:

- Fecha de emisión.
- Fecha de ingreso.
- Fecha de salida.
- Fecha de cada consumo.
- Precio por noche.
- Precio unitario de consumos.
- Importe de cada consumo.
- Total alojamiento.
- Total consumos.
- Subtotal.
- Descuento.
- Total final.

## Comentarios

También se ampliaron los comentarios dentro de:

- `Views/Facturacion/Factura.cshtml`
- `Views/Facturacion/CheckOut.cshtml`

Los comentarios explican prácticamente cada bloque,
cada etiqueta Razor y cada expresión importante.
