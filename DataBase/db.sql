create table public.settings (
    time timestamp default CURRENT_TIMESTAMP,
    id serial,
    device_name text,
    work_folder text,
    x_resolution int,
    y_resolution int,
    exposition int,
    x_frame_size int,
    y_frame_size int,
    x_frame_position int,
    y_frame_position int,
    primary key (id)
);

CREATE OR REPLACE FUNCTION get_settings (_device_name text) returns table (_work_folder text,
    _x_resolution int,
    _y_resolution int,
    _exposition int,
    _x_frame_size int,
    _y_frame_size int,
    _x_frame_position int,
    _y_frame_position int) as
    $$
        begin
            return query select work_folder,x_resolution,y_resolution,exposition,x_frame_size,y_frame_size,
            x_frame_position,_y_frame_position from public.settings where settings.device_name=_device_name having time=max(time);
        end;
    $$
LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION set_settings (_device_name text, _work_folder text,
    _x_resolution int,
    _y_resolution int,
    _exposition int,
    _x_frame_size int,
    _y_frame_size int,
    _x_frame_position int,
    _y_frame_position int) returns void as
    $$
        begin
            insert into settings (device_name,
                                  work_folder,
                                  x_resolution,
                                  y_resolution,
                                  exposition,
                                  x_frame_size,
                                  y_frame_size,
                                  x_frame_position,
                                  y_frame_position)
                              values
                                (_device_name ,
                                 _work_folder ,
                                _x_resolution ,
                                _y_resolution ,
                                _exposition ,
                                _x_frame_size ,
                                _y_frame_size ,
                                _x_frame_position ,
                                _y_frame_position );
        end;
    $$
LANGUAGE plpgsql;