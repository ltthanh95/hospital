export function niceCeil(value: number): number {
  if (value <= 0) return 1;
  const exponent = Math.floor(Math.log10(value));
  const magnitude = Math.pow(10, exponent);
  const residual = value / magnitude;

  let niceResidual: number;
  if (residual <= 1) niceResidual = 1;
  else if (residual <= 2) niceResidual = 2;
  else if (residual <= 5) niceResidual = 5;
  else niceResidual = 10;

  return niceResidual * magnitude;
}

export function roundedTopRectPath(x: number, y: number, w: number, h: number): string {
  if (h <= 0 || w <= 0) return '';
  const r = Math.min(4, w / 2, h);
  if (r <= 0.01) return `M${x},${y} h${w} v${h} h${-w} Z`;

  return [
    `M${x},${y + h}`,
    `L${x},${y + r}`,
    `A${r},${r} 0 0 1 ${x + r},${y}`,
    `L${x + w - r},${y}`,
    `A${r},${r} 0 0 1 ${x + w},${y + r}`,
    `L${x + w},${y + h}`,
    'Z',
  ].join(' ');
}

export function defaultValueFormatter(value: number): string {
  return value.toLocaleString();
}
