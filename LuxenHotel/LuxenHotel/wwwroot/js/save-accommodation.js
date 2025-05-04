document.addEventListener("DOMContentLoaded", function () {
  // Register FilePond plugins
  FilePond.registerPlugin(
    FilePondPluginImagePreview,
    FilePondPluginFileValidateType
  );

  // Initialize FilePond
  const pond = FilePond.create(document.getElementById("filepond"), {
    allowMultiple: true,
    maxFiles: 10,
    acceptedFileTypes: ["image/*"],
    labelIdle: `<div class="p-4 d-flex justify-content-between align-items-center">
      <div class="me-5 fs-3x text-primary">
        <i class="fa-solid fa-file-arrow-up"></i>
      </div>
      <div class="text-start">
        <h3 class="fs-5 fw-bold text-dark mb-1">Drop files here or click to upload.</h3>
        <span class="fs-7 text-muted">Upload up to 10 files</span>
      </div>
    </div>`,
    imagePreviewHeight: 170,
  });

  // Get existing media items (if in edit mode)
  const existingMediaContainer = document.getElementById(
    "existing-media-container"
  );
  const existingMediaItems = existingMediaContainer
    ? existingMediaContainer.querySelectorAll("[data-media-url]")
    : [];
  const isEditMode = existingMediaItems.length > 0;

  // Array to track existing media URLs
  const existingMedia = [];
  // Array to track media URLs to delete
  const mediaToDelete = [];

  // In edit mode, load existing images and create tracking inputs
  if (isEditMode) {
    console.log("Edit mode detected, loading existing images");

    // Create hidden input to track media files to delete
    const mediaToDeleteInput = document.createElement("input");
    mediaToDeleteInput.type = "hidden";
    mediaToDeleteInput.id = "mediaToDeleteInput";
    mediaToDeleteInput.name = "MediaToDelete";
    document.querySelector("form").appendChild(mediaToDeleteInput);

    // Process each existing media item
    existingMediaItems.forEach((item) => {
      const mediaUrl = item.getAttribute("data-media-url");
      if (mediaUrl) {
        // Add to tracking array
        existingMedia.push(mediaUrl);

        // Create a file name from the URL
        const fileName = mediaUrl.split("/").pop();

        // Fetch the image and add it to FilePond
        fetch(mediaUrl)
          .then((response) => {
            if (!response.ok) {
              throw new Error(
                `Failed to fetch image: ${response.status} ${response.statusText}`
              );
            }
            return response.blob();
          })
          .then((blob) => {
            // Create a File object
            const file = new File([blob], fileName, {
              type: getFileTypeFromUrl(mediaUrl),
            });

            // Add to FilePond and store the URL reference
            return pond.addFile(file);
          })
          .then((fileItem) => {
            // Store original URL as a custom property on the FilePond file item
            if (fileItem) {
              fileItem.existingUrl = mediaUrl;
            }
          })
          .catch((error) => {
            console.error(`Error loading image ${mediaUrl}:`, error);
          });
      }
    });

    // Initial update of the hidden input for media to delete
    updateMediaToDeleteInput();
  } else {
    console.log("Create mode detected");
  }

  // Function to update the hidden input with media files to delete
  function updateMediaToDeleteInput() {
    const input = document.getElementById("mediaToDeleteInput");
    if (input) {
      input.value = JSON.stringify(mediaToDelete);
    }
  }

  // Function to guess file type from URL
  function getFileTypeFromUrl(url) {
    const extension = url.split(".").pop().toLowerCase();
    switch (extension) {
      case "jpg":
      case "jpeg":
        return "image/jpeg";
      case "png":
        return "image/png";
      case "gif":
        return "image/gif";
      case "webp":
        return "image/webp";
      default:
        return "image/jpeg"; // Default fallback
    }
  }

  // When a file is removed from FilePond
  pond.on("removefile", (error, fileItem) => {
    // Check if it's an existing file (has existingUrl property)
    if (fileItem && fileItem.existingUrl) {
      // Add to the mediaToDelete array
      mediaToDelete.push(fileItem.existingUrl);
      // Remove from tracking array of existing media
      const index = existingMedia.indexOf(fileItem.existingUrl);
      if (index > -1) {
        existingMedia.splice(index, 1);
      }
      updateMediaToDeleteInput();
      console.log(`Marked file for deletion: ${fileItem.existingUrl}`);
    }
  });

  // Handle thumbnail deletion if checkbox is clicked
  const deleteThumbnailCheckbox = document.getElementById("deleteThumbnail");
  if (deleteThumbnailCheckbox) {
    deleteThumbnailCheckbox.addEventListener("change", function () {
      const thumbnailPreview = document.getElementById("thumbnailPreview");
      if (this.checked && thumbnailPreview) {
        thumbnailPreview.classList.add("opacity-50");
      } else if (thumbnailPreview) {
        thumbnailPreview.classList.remove("opacity-50");
      }
    });
  }

  // Handle form submission
  const form = document.querySelector("form");
  form.addEventListener("submit", function (e) {
    // Get all files from FilePond
    const allFiles = pond.getFiles();

    if (allFiles.length === 0) {
      // No files in FilePond, make sure realInput is empty too
      const realInput = document.getElementById("realMediaFiles");
      realInput.value = "";
      // Don't return yet, as we still need to handle media deletion
    }

    // Get the real file input element for new uploads
    const realInput = document.getElementById("realMediaFiles");
    const dataTransfer = new DataTransfer();

    // Count how many new files we find
    let newFileCount = 0;

    // Process all FilePond files
    allFiles.forEach((fileItem) => {
      // Check if this is a new file (doesn't have existingUrl property)
      if (!fileItem.existingUrl) {
        newFileCount++;
        // Add to the real file input
        dataTransfer.items.add(fileItem.file);
      }
    });

    console.log(
      `Processing ${newFileCount} new files, ${existingMedia.length} existing files, and ${mediaToDelete.length} files to delete`
    );

    // Set the files to the real input
    realInput.files = dataTransfer.files;

    // Make sure media to delete input is up to date if in edit mode
    if (isEditMode) {
      updateMediaToDeleteInput();
    }
  });
});

// Quill init
var quill = new Quill("#accommodation-desc", {
  theme: "snow",
  placeholder: "Type your text here...",
  modules: {
    toolbar: [
      [{ header: [1, 2, 3, false] }],
      ["bold", "italic", "underline"],
      ["link", "image"],
      [{ list: "ordered" }, { list: "bullet" }],
      ["clean"],
    ],
  },
});

const descriptionHidden = document.querySelector("#description-hidden");
quill.on("text-change", function () {
  descriptionHidden.value = quill.root.innerHTML;
});

// Use for edit
if (descriptionHidden.value) {
  quill.root.innerHTML = descriptionHidden.value;
}

let serviceIndex = 0;

function addServiceItem() {
  const wrapper = document.getElementById("services-wrapper");
  const template = document.getElementById("service-template");
  const clone = template.content.cloneNode(true);
  const index = serviceIndex++;

  // Update unique IDs and attributes for accordion
  const collapseId = `collapseService_${index}`;
  const header = clone.querySelector(".accordion-header");
  const collapse = clone.querySelector(".collapse");

  header.setAttribute("data-bs-target", `#${collapseId}`);
  header.setAttribute("aria-controls", collapseId);
  header.setAttribute("aria-expanded", "true");
  header.classList.remove("collapsed");

  collapse.id = collapseId;
  collapse.classList.add("show");

  // Update field names
  clone.querySelectorAll("[name]").forEach((input) => {
    const baseName = input.getAttribute("name").split(".")[1];
    input.setAttribute("name", `services[${index}].${baseName}`);
  });

  // Update input IDs for accessibility
  clone.querySelector("#service_name_0").id = `service_name_${index}`;
  clone.querySelector("#service_price_0").id = `service_price_${index}`;
  clone.querySelector(
    "#service_description_0"
  ).id = `service_description_${index}`;
  clone
    .querySelector(`label[for="service_name_0"]`)
    .setAttribute("for", `service_name_${index}`);
  clone
    .querySelector(`label[for="service_price_0"]`)
    .setAttribute("for", `service_price_${index}`);
  clone
    .querySelector(`label[for="service_description_0"]`)
    .setAttribute("for", `service_description_${index}`);

  // Update service index display
  clone.querySelector(".service-index").textContent = `Service #${index + 1}`;
  wrapper.appendChild(clone);
}

function removeServiceItem(button) {
  const item = button.closest(".service-item");
  item.remove();
}

// Initialize with one default service
window.addEventListener("DOMContentLoaded", () => {
  addServiceItem();
});
