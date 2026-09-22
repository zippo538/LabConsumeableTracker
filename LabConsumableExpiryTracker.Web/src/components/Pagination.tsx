import React, {useMemo} from 'react';

interface PaginationProps {
    page: number;
    onPageChange: (page: number) => void;
    
    totalItems?: number;
    pageSize?: number;
    onPageSizeChange?: (pageSize: number) => void;
    pageSizeOptions?: number[];
    totalPages?: number;
}

export const Pagination: React.FC<PaginationProps> = ({
    page,
    onPageChange,
    totalItems = 0,
    pageSize = 20,
    onPageSizeChange,
    pageSizeOptions = [10, 20, 50, 100],
    totalPages: totalPagesProp,
}) => {
    const totalPages =
        totalPagesProp ?? Math.max(1, Math.ceil(totalItems / pageSize));
    const firstItem = totalItems === 0 ? 0 : (page - 1) * pageSize + 1;
    const lastItem = Math.min(page * pageSize, totalItems);

    const visiblePages = useMemo(() => {
        const candidates = new Set([1, totalPages, page - 1, page, page + 1]);
        return [...candidates]
            .filter((n) => n >= 1 && n <= totalPages)
            .sort((a, b) => a - b);
    }, [page, totalPages]);

    if (totalItems === 0 && totalPages <= 1) return null;

    const changePage = (next: number) => {
        if (next < 1 || next > totalPages || next === page) return;
        onPageChange(next);
    };

    const changePageSize = (next: number) => {
        onPageChange(1);
        onPageSizeChange?.(next);
    };

    return (
        <div className="flex flex-col gap-4 border-t border-slate-200 px-6 py-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex flex-wrap items-center gap-3 text-[11px] text-slate-500">
                {totalItems > 0 && (
                    <span aria-live="polite">
                        Menampilkan {firstItem}–{lastItem} dari {totalItems} data
                    </span>
                )}

                {onPageSizeChange && (
                    <label className="flex items-center gap-2">
                        <span>Baris per halaman</span>
                        <select
                            value={pageSize}
                            onChange={(e) => changePageSize(Number(e.target.value))}
                            className="h-8 rounded-lg border border-slate-200 bg-white px-2 text-[11px] font-semibold text-slate-700 outline-none focus:border-teal-400 focus:ring-2 focus:ring-teal-100"
                        >
                            {pageSizeOptions.map((size) => (
                                <option key={size} value={size}>
                                    {size}
                                </option>
                            ))}
                        </select>
                    </label>
                )}
            </div>

            {totalPages > 1 && (
                <nav aria-label="Pagination" className="flex items-center gap-1.5">
                    <button
                        type="button"
                        onClick={() => changePage(page - 1)}
                        disabled={page <= 1}
                        className="h-8 rounded-lg border border-slate-200 bg-white px-3 text-[11px] font-semibold text-slate-600 transition hover:border-teal-300 hover:text-teal-700 disabled:cursor-not-allowed disabled:opacity-40"
                    >
                        Sebelumnya
                    </button>

                    {visiblePages.map((pageNumber, index) => {
                        const previous = visiblePages[index - 1];
                        return (
                            <div key={pageNumber} className="flex items-center gap-1">
                                {previous && pageNumber - previous > 1 && (
                                    <span className="px-1 text-slate-400">…</span>
                                )}
                                <button
                                    type="button"
                                    onClick={() => changePage(pageNumber)}
                                    aria-current={pageNumber === page ? 'page' : undefined}
                                    className={`grid h-8 min-w-8 place-items-center rounded-lg px-2 text-[11px] font-semibold transition ${
                                        pageNumber === page
                                            ? 'bg-[#13a9a7] text-white'
                                            : 'border border-slate-200 bg-white text-slate-600 hover:border-teal-300 hover:text-teal-700'
                                    }`}
                                >
                                    {pageNumber}
                                </button>
                            </div>
                        );
                    })}

                    <button
                        type="button"
                        onClick={() => changePage(page + 1)}
                        disabled={page >= totalPages}
                        className="h-8 rounded-lg border border-slate-200 bg-white px-3 text-[11px] font-semibold text-slate-600 transition hover:border-teal-300 hover:text-teal-700 disabled:cursor-not-allowed disabled:opacity-40"
                    >
                        Berikutnya
                    </button>
                </nav>
            )}
        </div>
    );
};