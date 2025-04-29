// FilePond init
FilePond.registerPlugin(FilePondPluginImagePreview);

FilePond.create(document.getElementById("filepond"), {
  allowMultiple: true,
  maxFiles: 10,
  acceptedFileTypes: ["image/*"],
  labelIdle: `<div class="ms-4">
        <h3 class="fs-5 fw-bold text-gray-900 mb-1">Drop files here or click to upload.</h3>
        <span class="fs-7 fw-semibold text-gray-500">Upload up to 10 files</span>
    </div>`,
  imagePreviewHeight: 170,
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

// Đồng bộ nội dung Quill với input ẩn
const descriptionHidden = document.querySelector("#description-hidden");
quill.on("text-change", function () {
  descriptionHidden.value = quill.root.innerHTML;
});

// Set giá trị ban đầu nếu có (ví dụ khi edit)
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
  header.setAttribute("aria-expanded", "true"); // Set expanded state
  header.classList.remove("collapsed"); // Remove collapsed class to expand

  collapse.id = collapseId;
  collapse.classList.add("show"); // Add show class to expand the body

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
