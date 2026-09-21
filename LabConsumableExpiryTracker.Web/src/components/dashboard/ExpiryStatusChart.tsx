import type { FC } from 'react';
import type { DashboardSummary } from '../../types/dashboard.types';

interface ExpiryStatusChartProps {
  summary: DashboardSummary;
}

export const ExpiryStatusChart: FC<ExpiryStatusChartProps> = ({ summary}) => {
  const values = [summary.validLotCount, summary.expiringSoonLotCount, summary.expiredLotCount];
  const knownValues = values.filter((value): value is number => typeof value === 'number');
  const maximum = Math.max(1, ...knownValues);
  const bars = [
    { label: 'Valid', value: summary.validLotCount, color: 'bg-[#22c55e]' },
    { label: 'Soon', value: summary.expiringSoonLotCount, color: 'bg-[#fca311]' },
    { label: 'Expired', value: summary.expiredLotCount, color: 'bg-[#e52528]' },
  ];

  return <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-[0_1px_3px_rgb(15_23_42/0.03)]" aria-labelledby="expiry-chart-title">
    <h3 id="expiry-chart-title" className="text-[16px] font-bold tracking-tight text-slate-800">Status expiry</h3>
    <p className="mt-1 text-[11px] text-slate-400">Lot berdasarkan evaluasi server</p>
    <div className="mt-8 flex h-[132px] items-end justify-around gap-4" role="img" aria-label={`Lot mendekati expiry: ${summary.expiringSoonLotCount}; lot expired: ${summary.expiredLotCount}; lot valid: ${summary.validLotCount ?? 'tidak tersedia'}`}>
      {bars.map(({ label, value, color }) => {
        const height = value === undefined ? 0 : Math.max(10, Math.round((value / maximum) * 100));
        return <div key={label} className="flex h-full min-w-0 flex-1 flex-col items-center justify-end">
          <span className="mb-2 text-[11px] font-bold text-slate-600">{value ?? '—'}</span>
          <div className="flex h-[88px] w-full max-w-[38px] items-end overflow-hidden rounded-lg bg-[#eef2f5]">
            {value !== undefined && <span className={`w-full rounded-lg ${color}`} style={{ height: `${height}%` }} />}
          </div>
          <span className="mt-2 text-[10px] font-medium text-slate-500">{label}</span>
        </div>;
      })}
    </div>
    {summary.validLotCount === undefined && <p className="mt-5 text-[10px] leading-4 text-slate-400">Jumlah lot valid belum tersedia dari endpoint dashboard.</p>}
  </section>;
};
