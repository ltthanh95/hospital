import { Component, computed, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { defaultValueFormatter, niceCeil, roundedTopRectPath } from './chart-utils';

export interface ChartSeries {
  name: string;
  color: string;
  values: number[];
}

interface Bar {
  path: string;
  color: string;
  catIndex: number;
  seriesIndex: number;
  value: number;
  seriesName: string;
  category: string;
  labelX: number;
  labelY: number;
}

interface GridLine {
  value: number;
  y: number;
}

const VIEW_WIDTH = 640;
const MARGIN_LEFT = 52;
const MARGIN_RIGHT = 12;
const MARGIN_TOP = 16;
const MARGIN_BOTTOM = 26;
const GROUP_GAP = 10;
const BAR_GAP = 3;

@Component({
  selector: 'app-bar-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bar-chart.html',
})
export class BarChartComponent {
  categories = input<string[]>([]);
  series = input<ChartSeries[]>([]);
  categoryColors = input<string[] | null>(null);
  formatValue = input<(value: number) => string>(defaultValueFormatter);
  height = input(260);

  showTable = signal(false);
  hovered = signal<{ cat: number; series: number } | null>(null);

  readonly viewWidth = VIEW_WIDTH;
  readonly marginLeft = MARGIN_LEFT;
  readonly marginRight = MARGIN_RIGHT;

  plotWidth = computed(() => VIEW_WIDTH - MARGIN_LEFT - MARGIN_RIGHT);
  plotHeight = computed(() => this.height() - MARGIN_TOP - MARGIN_BOTTOM);

  maxValue = computed(() => {
    const all = this.series().flatMap(s => s.values);
    const max = all.length ? Math.max(0, ...all) : 0;
    return niceCeil(max || 1);
  });

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

  groupCenters = computed(() => {
    const cats = this.categories();
    const groupWidth = this.plotWidth() / (cats.length || 1);
    return cats.map((cat, i) => ({ category: cat, x: MARGIN_LEFT + (i + 0.5) * groupWidth }));
  });

  bars = computed<Bar[]>(() => {
    const cats = this.categories();
    const seriesArr = this.series();
    const max = this.maxValue();
    const plotHeight = this.plotHeight();
    const groupWidth = this.plotWidth() / (cats.length || 1);
    const seriesCount = seriesArr.length || 1;
    const innerWidth = groupWidth - GROUP_GAP;
    const barWidth = Math.max((innerWidth - (seriesCount - 1) * BAR_GAP) / seriesCount, 2);
    const overrideColors = this.categoryColors();
    const useOverride = !!overrideColors && seriesArr.length === 1;

    const result: Bar[] = [];
    cats.forEach((cat, ci) => {
      const groupX = MARGIN_LEFT + ci * groupWidth + GROUP_GAP / 2;
      seriesArr.forEach((s, si) => {
        const value = s.values[ci] ?? 0;
        const barHeight = max > 0 ? (value / max) * plotHeight : 0;
        const x = groupX + si * (barWidth + BAR_GAP);
        const y = MARGIN_TOP + plotHeight - barHeight;
        const color = useOverride ? (overrideColors![ci] ?? s.color) : s.color;
        result.push({
          path: roundedTopRectPath(x, y, barWidth, barHeight),
          color,
          catIndex: ci,
          seriesIndex: si,
          value,
          seriesName: s.name,
          category: cat,
          labelX: x + barWidth / 2,
          labelY: y,
        });
      });
    });
    return result;
  });

  showLegend = computed(() => {
    const s = this.series();
    return s.length > 1 || (!!this.categoryColors() && s.length === 1 && this.categories().length > 1);
  });

  legendItems = computed(() => {
    const s = this.series();
    const overrideColors = this.categoryColors();
    if (overrideColors && s.length === 1) {
      return this.categories().map((c, i) => ({ label: c, color: overrideColors[i] ?? s[0].color }));
    }
    return s.map(item => ({ label: item.name, color: item.color }));
  });

  hoveredBar = computed(() => {
    const h = this.hovered();
    if (!h) return null;
    return this.bars().find(b => b.catIndex === h.cat && b.seriesIndex === h.series) ?? null;
  });

  setHovered(bar: Bar) {
    this.hovered.set({ cat: bar.catIndex, series: bar.seriesIndex });
  }

  clearHovered() {
    this.hovered.set(null);
  }

  isHovered(bar: Bar) {
    const h = this.hovered();
    return !!h && h.cat === bar.catIndex && h.series === bar.seriesIndex;
  }

  toggleTable() {
    this.showTable.update(v => !v);
  }
}
