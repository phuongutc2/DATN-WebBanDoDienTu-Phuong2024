
$(document).ready(function () {
    $('.btn-close').click(function () {
        $('.mini_cart_box').removeClass('openCart');
    });
});

var cart = {
    init: function () {
        cart.regEvents();
    },
    regEvents: function () {

        $('.btn-addCart').off('click').on('click', function () {
            
            $.ajax({
                type: 'GET',
                url: '/Cart/AddCart',
                data: {
                    product_ID: $(this).data('id'),
                    quantity: $(this).data('quantity'),
                    color: $('#color').val(),
                    size: $('#size').val()
                },
                dataType: 'Json',
                contentType: "application/json; charset=utf-8",
                success: function (res) {
                    if (res.status == true) {
                        var count = $('.cart_count').text();
                        var Soluong = parseInt(count) + 1;
                        $('.mini_cart_box').css('display', 'block');
                        $('.cart_count').text(Soluong);
                        $('.mini_cart_box').addClass('openCart');
                        //$('.mini_cart_box').delay(4000).slideUp(500);
                        $("html, body").animate({ scrollTop: "0" }, 500);

                        
                    } else {
                        PNotify.error({
                            title: 'THÔNG BÁO!!',
                            text: 'Sản phẩm bạn chọn đã hết hoặc chỉ còn 1 sản phẩm.'
                        });
                    }

                }
            });
        });


        $('.cart_quantity_down').off('click').on('click', function () {
            var id = $(this).data('id');
            var quantity = $('#quantity_' + id);
            var count = parseInt(quantity.val()) - 1;

            //số lượng = 0
            if (count == 0) {
                PNotify.error({
                    title: 'THÔNG BÁO!!',
                    text: 'Số lượng sản phẩm không thể bằng 0.'
                });
                $('#quantity_' + id).val(count + 1);
            } else {
                $('#quantity_' + id).val(count);

                //Phương thức Ajax dùng để đẩy lên Controller
                $.ajax({
                    url: '/Cart/Edit',
                    data: {
                        product_ID: id,
                        quantity: count
                    },
                    dataType: 'Json',
                    type: 'POST',
                    success: function (res) {
                        if (res.status == true) {
                            window.location.href = "/Cart";
                        }
                    }
                });
            }
        });

        $('.cart_quantity_up').off('click').on('click', function () {

            var id = $(this).data('id');
            var quantity = $('#quantity_' + id);
            var count = parseInt(quantity.val()) + 1;
            $('#quantity_' + id).val(count);
            //Phương thức Ajax dùng để đẩy lên Controller
            $.ajax({
                url: '/Cart/Edit',
                data: {
                    product_ID: id,
                    quantity: count
                },
                dataType: 'Json',
                type: 'POST',
                success: function (res) {
                    if (res.status == true) {
                        window.location.href = "/Cart";
                    }
                }
            });

        });


        $('.cart_quantity_delete').off('click').on('click', function () {
            $.ajax({
                data: { id: $(this).data('id') },
                url: '/Cart/Delete',
                dataType: 'Json',
                type: 'POST',
                success: function (res) {
                    if (res.status == true) {
                        window.location.href = "/Cart";
                    }
                }
            });
        });


    }

}

cart.init();
