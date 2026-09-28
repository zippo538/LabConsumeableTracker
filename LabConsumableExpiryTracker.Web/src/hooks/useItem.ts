import {
    useEffect,
    useMemo,
    useState,
} from "react";

import {
    useMutation,
    useQuery,
    useQueryClient,
    keepPreviousData,
} from "@tanstack/react-query";

import {itemApi} from "../api/itemApi";

import type {
    ItemDto,
    CreateItemPayload,
    UpdateItemPayload,
} from "../types/item.types";

export const useItem = () => {
    const queryClient = useQueryClient();


    const [page, setPage] = useState(1);
    const [pageSize, setPageSizeState] = useState(20);
    
    const setPageSize = (nextPageSize: number) => {
        setPageSizeState(nextPageSize);
        setPage(1);
    };

    const [searchQuery, setSearchQuery] = useState("");
    const [selectedUnit, setSelectedUnit] = useState("ALL");
    const [selectedStatus, setSelectedStatus] = useState("ALL");

    const hasActiveFilter =
        searchQuery.trim() !== "" ||
        selectedUnit !== "ALL" ||
        selectedStatus !== "ALL";


    // ---- getAll: powers the summary card AND client-side filtering ----
    // The backend /Item/paged endpoint doesn't accept filter params yet,
    // so whenever a filter is active we filter+paginate this full dataset
    // ourselves instead of trusting the server page.
    const {
        data: allItems = [],
        isLoading: isLoadingAllItems,
    } = useQuery<ItemDto[]>({
        queryKey: ["items", "all"],
        queryFn: itemApi.getAll,
    });

    const filteredAllItems = useMemo(() => {
        return allItems.filter((item) => {
            const query = searchQuery
                .trim()
                .toLowerCase();

            const matchQuery =
                !query ||
                item.name.toLowerCase().includes(query) ||
                item.code.toLowerCase().includes(query);

            const matchUnit =
                selectedUnit === "ALL" ||
                item.baseUnit.toLowerCase() ===
                selectedUnit.toLowerCase();

            const matchStatus =
                selectedStatus === "ALL" ||
                (selectedStatus === "ACTIVE" && true) ||
                (selectedStatus === "LOW_STOCK" &&
                    item.isLowStock);

            return (
                matchQuery &&
                matchUnit &&
                matchStatus
            );
        });
    }, [
        allItems,
        searchQuery,
        selectedUnit,
        selectedStatus,
    ]);


    // ---- getPaged: real backend pagination, used when there's no filter ----
    const {
        data: pagedResult,
        isLoading: isLoadingPaged,
        isFetching: isFetchingPaged,
    } = useQuery({
        queryKey: ["items", "paged", page, pageSize],
        queryFn: () => itemApi.getPaged(page, pageSize),
        enabled: !hasActiveFilter,
        placeholderData: keepPreviousData,
    });


    // ---- Merge the two sources into what the table actually renders ----
    const totalCount = hasActiveFilter
        ? filteredAllItems.length
        : pagedResult?.totalCount ?? 0;

    const totalPages = Math.max(
        1,
        Math.ceil(totalCount / pageSize),
    );

    const filteredItems = useMemo(() => {
        if (hasActiveFilter) {
            const startIndex = (page - 1) * pageSize;
            return filteredAllItems.slice(
                startIndex,
                startIndex + pageSize,
            );
        }

        return pagedResult?.items ?? [];
    }, [
        hasActiveFilter,
        filteredAllItems,
        pagedResult,
        page,
        pageSize,
    ]);

    const isLoading = hasActiveFilter
        ? isLoadingAllItems
        : isLoadingPaged;


    // Reset to page 1 whenever a filter changes, so we don't land on an
    // out-of-range page for the new (filtered) result set.
    useEffect(() => {
        setPage(1);
    }, [
        searchQuery,
        selectedUnit,
        selectedStatus,
    ]);

    // Clamp page if it becomes out of range (e.g. last item on last page
    // got deleted, or a filter shrank the result set).
    useEffect(() => {
        if (page > totalPages) {
            setPage(totalPages);
        }
    }, [
        page,
        totalPages,
    ]);


    const invalidateItemQueries = () => {
        queryClient.invalidateQueries({
            queryKey: ["items", "all"],
        });
        queryClient.invalidateQueries({
            queryKey: ["items", "paged"],
        });
    };

    const createMutation = useMutation({
        mutationFn: (
            payload: CreateItemPayload,
        ) => itemApi.create(payload),

        onSuccess: invalidateItemQueries,
    });

    const updateMutation = useMutation({
        mutationFn: (
            payload: UpdateItemPayload,
        ) => itemApi.update(payload),

        onSuccess: invalidateItemQueries,
    });

    const deleteMutation = useMutation({
        mutationFn: (id: string) =>
            itemApi.delete(id),

        onSuccess: invalidateItemQueries,
    });


    // ---- Card summary, always derived from the full dataset ----
    const stats = useMemo(() => {
        const totalItems =
            allItems.length;

        const lowStockCount =
            allItems.filter(
                (item) => item.isLowStock,
            ).length;

        const uniqueUnits =
            new Set(
                allItems.map(
                    (item) => item.baseUnit,
                ),
            ).size;

        return {
            totalActive: totalItems,
            lowStock: lowStockCount,
            uniqueUnitsCount:
                uniqueUnits || 0,
        };
    }, [allItems]);


    const availableUnits = useMemo(() => {
        return Array.from(
            new Set(
                allItems
                    .map(
                        (item) => item.baseUnit,
                    )
                    .filter(Boolean),
            ),
        );
    }, [allItems]);


    const createItem = async (
        payload: CreateItemPayload,
    ) => {
        await createMutation.mutateAsync(
            payload,
        );
    };

    const updateItem = async (
        payload: UpdateItemPayload,
    ) => {
        await updateMutation.mutateAsync(
            payload,
        );
    };

    const deleteItem = async (
        id: string,
    ) => {
        return deleteMutation.mutateAsync(
            id,
        );
    };


    return {
        items: allItems,
        filteredItems,
        page,
        pageSize,
        setPageSize,
        totalCount,
        totalPages,
        setPage,
        isLoading,
        isFetching: isFetchingPaged,
        isError: false,
        error: null,
        stats,
        availableUnits,
        searchQuery,
        setSearchQuery,
        selectedUnit,
        setSelectedUnit,
        selectedStatus,
        setSelectedStatus,
        createItem,
        updateItem,
        deleteItem,
        isCreating:
        createMutation.isPending,
        isUpdating:
        updateMutation.isPending,
        isDeleting:
        deleteMutation.isPending,
    };
};