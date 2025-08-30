import "../App.css"

export default function FileList({files, loading, error}) {

  return(
    <div>
      {loading && <p>Loading files...</p>}
      {error && <p className="text-red-500">{error}</p>}
      <h2>File List</h2>
      {/* Table should be mobile responsive */}
      <div className="overflow-x-auto">
      <table className="min-w-full bg-white border border-gray-200">
        <thead className="bg-gray-100">
          <tr className="text-left">
            <th className="px-4 py-2">Name</th>
            <th className="px-4 py-2">Type</th>
            <th className="px-4 py-2">Size</th>
            <th className="px-4 py-2">Created At</th>
          </tr>
        </thead>
        <tbody>
          {files.map(file => (
            <tr className="text-left" key={file.Id}>
              <td className="border px-4 py-2">{file.Name}</td>
              <td className="border px-4 py-2">{file.Type}</td>
              <td className="border px-4 py-2">{(file.Size/(1024 * 1024)).toFixed(2)} MB</td>
              <td className="border px-4 py-2">{file.UploadTime}</td>
            </tr>
          ))}
        </tbody>
      </table>
      </div>
    </div>
  );
}














































