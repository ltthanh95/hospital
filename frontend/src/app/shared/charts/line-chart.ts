import { Component, computed, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { defaultValueFormatter, niceCeil } from './chart-utils';

interface Point {
  x: number;
  y: number;
  value: number;
  category: string;
}

interface GridLine {
  value: number;
  y: number;
}

const VIEW_WIDTH = 640;
const MARGIN_LEFT = 56;
const MARGIN_RIGHT = 12;
const MARGIN_TOP = 16;
const MARGIN_BOTTOM = 26;

@Component({
  selector: 'app-line-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './line-chart.html',
})
export class LineChartComponent {
  categories = input<string[]>([]);
  data = input<number[]>([]);
  color = input('#256abf');
  formatValue = input<(value: number) => string>(defaultValueFormatter);
  height = input(260);

  showTable = signal(false);
  hoveredIndex = signal<number | null>(null);

  readonly viewWidth = VIEW_WIDTH;
  readonly marginLeft = MARGIN_LEFT;
  readonly marginRight = MARGIN_RIGHT;
  readonly marginTop = MARGIN_TOP;

  plotWidth = computed(() => VIEW_WIDTH - MARGIN_LEFT - MARGIN_RIGHT);
  plotHeight = computed(() => this.height() - MARGIN_TOP - MARGIN_BOTTOM);

  maxValue = computed(() => niceCeil(Math.max(0, ...this.data()) || 1));

  gridLines = computed<GridLine[]>(() => {
    const max = this.maxValue();
    const steps = 4;
    const plotHeight = this.plotHeight();
    return Array.from({ length: steps + 1 }, (_, i) => ({
      value: (max / steps) * i,
      y: MARGIN_TOP + plotHeight * (1 - i / steps),
    }));
  });

  labelStep = computed(() => (this.categories().length > 8 ? 2 : 1));

  points = computed<Point[]>(() => {
    const cats = this.categories();
    const values = this.data();
    const max = this.maxValue();
    const plotHeight = this.plotHeight();
    const plotWidth = this.plotWidth();
    const stepX = cats.length > 1 ? plotWidth / (cats.length - 1) : 0;

    return cats.map((cat, i) => {
      const value = values[i] ?? 0;
      const x = MARGIN_LEFT + (cats.length > 1 ? i * stepX : plotWidth / 2);
      const y = MARGIN_TOP + plotHeight - (max > 0 ? (value / max) * plotHeight : 0);
      return { x, y, value, category: cat };
    });
  });

  linePath = computed(() => {
    const pts = this.points();
    if (!pts.length) return '';
    return pts.map((p, i) => `${i === 0 ? 'M' : 'L'}${p.x},${p.y}`).join(' ');
  });

  areaPath = computed(() => {
    const pts = this.points();
    if (!pts.length) return '';
    const baselineY = MARGIN_TOP + this.plotHeight();
    const first = pts[0];
    const last = pts[pts.length - 1];
    const line = pts.map((p, i) => `${i === 0 ? 'M' : 'L'}${p.x},${p.y}`).join(' ');
    return `${line} L${last.x},${baselineY} L${first.x},${baselineY} Z`;
  });

  hoveredPoint = computed(() => {
    const i = this.hoveredIndex();
    return i === null ? null : (this.points()[i] ?? null);
  });

  setHovered(i: number) {
    this.hoveredIndex.set(i);
  }

  clearHovered() {
    this.hoveredIndex.set(null);
  }

  toggleTable() {
    this.showTable.update(v => !v);
  }
}
