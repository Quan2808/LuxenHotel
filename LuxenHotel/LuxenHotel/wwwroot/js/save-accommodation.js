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
});

var quill = new Quill("#service-desc", {
  theme: "snow",
  placeholder: "Enter service description (optional)",
});
// Status indicator
document.addEventListener("DOMContentLoaded", function () {
  const statusIndicator = document.getElementById(
    "kt_ecommerce_add_product_status"
  );
  const statusRadios = document.querySelectorAll(
    'input[name="product_status"]'
  );

  function updateStatusIndicator(value) {
    statusIndicator.classList.remove(
      "bg-success",
      "bg-danger",
      "bg-warning",
      "bg-dark"
    );

    switch (value) {
      case "published":
        statusIndicator.classList.add("bg-success");
        break;
      case "unpublished":
        statusIndicator.classList.add("bg-danger");
        break;
      case "maintenance":
        statusIndicator.classList.add("bg-warning");
        break;
      case "fully_booked":
        statusIndicator.classList.add("bg-dark");
        break;
    }
  }

  statusRadios.forEach(function (radio) {
    radio.addEventListener("change", function () {
      updateStatusIndicator(this.value);
    });
  });

  const checkedRadio = document.querySelector(
    'input[name="product_status"]:checked'
  );
  if (checkedRadio) {
    updateStatusIndicator(checkedRadio.value);
  }
});

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
