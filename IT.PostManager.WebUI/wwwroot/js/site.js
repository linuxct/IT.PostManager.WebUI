$(document).ready(function () {
    let replaceableList = $("td[data-mustreplace='true']");
    if (replaceableList.length) {
        replaceableList.each(function(i, el){
            $.get( "/Operation/GetPostDataAjax", { hashpath: el.dataset.hashpath } )
                .done(function(data) {
                    let currentEl = $(el);
                    if (data.disabled){
                        currentEl.parent().remove();
                        return;
                    }
                    if (data.date === "0001-01-01"){
                        currentEl.text("No time");
                        return;
                    }
                    currentEl.text(data.date);
                    let postDate = new Date(data.date);
                    let currentDate = new Date();
                    currentDate.setHours(0,0,0,0);
                    if (postDate < currentDate){
                        currentEl.parent().remove();
                    }
                })
                .fail(function() {
                    let currentEl = $(el);
                    currentEl.text("No time");
                });
        });
    }

    let downloadButton = $('button.lockable');
    if (downloadButton.length){
        downloadButton.click(function(){
            downloadButton.attr('disabled', 'disabled');
        });
    }

    let formPostInput = $('input.lockable');
    if (formPostInput.length){
        formPostInput.click(function(){
            formPostInput.attr('disabled', 'disabled');
            formPostInput.closest('form').submit();
        });
    }
});