document.addEventListener('DOMContentLoaded', function () {
    const downloadContainer = document.querySelector('#security-reports--download-container');
    if (downloadContainer) {
        const processDownload = downloadContainer.getAttribute('data-process-download');

        // Only trigger download if the flag Model.ProcessDownload is true
        if (processDownload === 'true') {
            const url = downloadContainer.getAttribute('data-url');
            const confirmationUrl = downloadContainer.getAttribute('data-confirmation-url');

            // Start the download process automatically
            axios({
                url: url,
                method: 'GET',
                responseType: 'blob',
                withCredentials: true
            }).then((response) => {
                if (response.status === 200) {
                    const contentDisposition = response.headers['content-disposition'];
                    const match = contentDisposition.match(/filename\*=UTF-8''([\w%\-\.]+)(?:; ?|$)/i);
                    const filename = match[1];

                    const fileURL = window.URL.createObjectURL(new Blob([response.data]));
                    const link = document.createElement('a');
                    link.href = fileURL;
                    link.setAttribute('download', filename);
                    document.body.appendChild(link);
                    link.click();
                }

                // Redirect after the download
                window.location = confirmationUrl;
            }).catch((error) => {
                console.error('Download failed:', error);
            });
        }
    }
});
