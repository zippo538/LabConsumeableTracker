import {
    useEffect,
    useMemo,
    useState,
} from "react";

import {
    useMutation,
    useQuery,
    useQueryClient,
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
    const pageSize = 20;

    const [searchQuery, setSearchQuery] = useState("");
    const [selectedUnit, setSelectedUnit] = useState("ALL");
    const [selectedStatus, setSelectedStatus] = useState("ALL");


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


    const totalCount = filteredAllItems.length;

    const totalPages = Math.max(
        1,
        Math.ceil(totalCount / pageSize),
    );


    const filteredItems = useMemo(() => {
        const startIndex =
            (page - 1) * pageSize;

        return filteredAllItems.slice(
            startIndex,
            startIndex + pageSize,
        );
    }, [
        filteredAllItems,
        page,
        pageSize,
    ]);


    useEffect(() => {
        setPage(1);
    }, [
        searchQuery,
        selectedUnit,
        selectedStatus,
    ]);


    useEffect(() => {
        if (page > totalPages) {
            setPage(totalPages);
        }
    }, [
        page,
        totalPages,
    ]);


    const isLoading =
        isLoadingAllItems;

    const createMutation = useMutation({
        mutationFn: (
            payload: CreateItemPayload,
        ) => itemApi.create(payload),

        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ["items"],
            });
        },
    });

    const updateMutation = useMutation({
        mutationFn: (
            payload: UpdateItemPayload,
        ) => itemApi.update(payload),

        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ["items"],
            });
        },
    });

    const deleteMutation = useMutation({
        mutationFn: (id: string) =>
            itemApi.delete(id),

        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ["items"],
            });
        },
    });


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
        totalCount,
        totalPages,
        setPage,
        isLoading,
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