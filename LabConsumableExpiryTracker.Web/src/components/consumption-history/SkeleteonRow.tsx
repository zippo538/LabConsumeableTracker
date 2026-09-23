export default function SkeletonRow() {
  return (
    <tr>
      {Array.from({ length: 6 }).map((_, index) => (
        <td key={index} className="px-6 py-4">
          <div className="h-4 w-full max-w-[120px] animate-pulse rounded bg-slate-100" />
        </td>
      ))}
    </tr>
  )
}